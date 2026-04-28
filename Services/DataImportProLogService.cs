using Dapper;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class DataImportProLogService : IDataImportProLogService
    {
        private readonly ConnectionStrings _connectionStrings;

        public DataImportProLogService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(DataImportProLogFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_DataImportProLog", new
                    {
                        TypeRequest = "SELECT",
                        filtro.DataImportProLogId,
                        filtro.Status,
                        filtro.UserId,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public dynamic Delete(int id, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_DataImportProLog", new
                    {
                        TypeRequest = "DELETE",
                        DataImportProLogId = id,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public async Task<ImportacaoResultado> ImportarAsync(Stream stream, string fileName, int? userId)
        {
            var erros = new List<string>();
            int dataImportProLogId = 0;
            int totalProf = 0, totalCli = 0, sucessos = 0;

            // Configurar EPPlus para uso não-comercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                con.Open();
                try
                {
                    // 1. Criar registro de log
                    var logResult = con.QueryFirstOrDefault("sp_DataImportProLog_Create",
                        new { FileName = fileName, FileType = "AMBOS", UserId = userId },
                        commandType: CommandType.StoredProcedure);
                    dataImportProLogId = (int)(logResult?.DataImportProLogId ?? 0);

                    // 2. Ler Excel com EPPlus
                    stream.Position = 0;
                    using var package = new ExcelPackage(stream);
                    var wsProfissionais = package.Workbook.Worksheets["Profissionais"];
                    var wsClientes = package.Workbook.Worksheets["Clientes"];

                    // 3. Inserir profissionais
                    if (wsProfissionais != null)
                    {
                        int rows = wsProfissionais.Dimension?.Rows ?? 0;
                        totalProf = rows > 1 ? rows - 1 : 0;
                        for (int i = 2; i <= rows; i++)
                        {
                            var nome = wsProfissionais.Cells[i, 4].Text?.Trim(); // coluna D = Nome
                            if (string.IsNullOrWhiteSpace(nome))
                            {
                                erros.Add($"Linha {i} (Profissionais): Nome é obrigatório.");
                                continue;
                            }
                            con.Execute("sp_DataImportProProfissional_Insert", new
                            {
                                DataImportProLogId = dataImportProLogId,
                                ID              = wsProfissionais.Cells[i, 1].Text,
                                CodProfissional = wsProfissionais.Cells[i, 2].Text,
                                Email           = wsProfissionais.Cells[i, 3].Text,
                                Nome            = nome,
                                Celular         = wsProfissionais.Cells[i, 5].Text,
                                Whats           = wsProfissionais.Cells[i, 6].Text,
                                CodEquipe       = wsProfissionais.Cells[i, 7].Text,
                                Vendedor        = wsProfissionais.Cells[i, 8].Text?.Trim().ToLower() == "sim",
                                CodSuperior     = wsProfissionais.Cells[i, 9].Text
                            }, commandType: CommandType.StoredProcedure);
                            sucessos++;
                        }
                    }

                    // 4. Inserir clientes
                    if (wsClientes != null)
                    {
                        int rows = wsClientes.Dimension?.Rows ?? 0;
                        totalCli = rows > 1 ? rows - 1 : 0;
                        for (int i = 2; i <= rows; i++)
                        {
                            var nomeFantasia = wsClientes.Cells[i, 3].Text?.Trim(); // coluna C = NomeFantasia
                            if (string.IsNullOrWhiteSpace(nomeFantasia))
                            {
                                erros.Add($"Linha {i} (Clientes): NomeFantasia é obrigatório.");
                                continue;
                            }
                            con.Execute("sp_DataImportProCliente_Insert", new
                            {
                                DataImportProLogId = dataImportProLogId,
                                ID           = wsClientes.Cells[i, 1].Text,
                                CodCliente   = wsClientes.Cells[i, 2].Text,
                                NomeFantasia = nomeFantasia,
                                CNPJ         = wsClientes.Cells[i, 4].Text,
                                CodVendedor  = wsClientes.Cells[i, 5].Text
                            }, commandType: CommandType.StoredProcedure);
                            sucessos++;
                        }
                    }

                    // 5. Finalizar log
                    con.Execute("sp_DataImportProLog_Finalize", new
                    {
                        DataImportProLogId = dataImportProLogId,
                        TotalRows     = totalProf + totalCli,
                        ProcessedRows = sucessos,
                        ErrorRows     = erros.Count
                    }, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    if (dataImportProLogId > 0)
                        con.Execute("SP_Adm_DataImportProLog", new
                        {
                            TypeRequest = "UPDATE",
                            DataImportProLogId = dataImportProLogId,
                            Status = "ERROR",
                            ErrorMessage = ex.Message
                        }, commandType: CommandType.StoredProcedure);
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }

            return new ImportacaoResultado { Success = sucessos, Errors = erros };
        }
    }
}
