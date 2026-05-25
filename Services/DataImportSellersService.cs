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
    public class DataImportSallersService : IDataImportSallersService
    {
        private readonly ConnectionStrings _connectionStrings;

        public DataImportSallersService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(DataImportSallersFilterModel filtro, string tokenUsuario)
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            try
            {
                con.Open();
                return con.Query("SP_Adm_DataImportSallersLog", new
                {
                    TypeRequest = "SELECT",
                    filtro.DataImportSallersLogId,
                    filtro.Status,
                    filtro.UserId,
                    token_usuario = tokenUsuario
                }, commandType: CommandType.StoredProcedure).ToList();
            }
            finally
            {
                con.Close();
            }
        }

        public dynamic Delete(int id, string tokenUsuario)
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            try
            {
                con.Open();
                return con.Query("SP_Adm_DataImportSallersLog", new
                {
                    TypeRequest = "DELETE",
                    DataImportSallersLogId = id,
                    token_usuario = tokenUsuario
                }, commandType: CommandType.StoredProcedure).ToList();
            }
            finally
            {
                con.Close();
            }
        }

        public async Task<ImportacaoSallersResultado> ImportarAsync(Stream stream, string fileName, int? userId)
        {
            var erros = new List<string>();
            int logId = 0;
            int totalRows = 0, sucessos = 0;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var con = new SqlConnection(_connectionStrings.Default);
            con.Open();
            try
            {
                // 1. Criar registro de log
                var logResult = con.QueryFirstOrDefault("sp_DataImportSallersLog_Create",
                    new { FileName = fileName, UserId = userId },
                    commandType: CommandType.StoredProcedure);
                logId = (int)(logResult?.DataImportSallersLogId ?? 0);

                // 2. Ler Excel com EPPlus
                stream.Position = 0;
                using var package = new ExcelPackage(stream);
                var ws = package.Workbook.Worksheets["Tabelao"];

                if (ws == null)
                {
                    erros.Add("A planilha deve conter a aba \"Tabelao\".");
                }
                else
                {
                    int rows = ws.Dimension?.Rows ?? 0;
                    totalRows = rows > 1 ? rows - 1 : 0;

                    // Colunas: A=ID, B=CodCliente, C=NomeFantasia, D=CNPJ,
                    //          E=CodProfissional, F=Email, G=Nome, H=Celular,
                    //          I=CodEquipe, J=Vendedor, K=CodSuperior,
                    //          L=SupNome, M=SupEmail, N=SupCelular
                    for (int i = 2; i <= rows; i++)
                    {
                        var nome = ws.Cells[i, 7].Text?.Trim(); // coluna G = Nome
                        if (string.IsNullOrWhiteSpace(nome))
                        {
                            erros.Add($"Linha {i}: Nome é obrigatório.");
                            continue;
                        }

                        var vendedorText = ws.Cells[i, 10].Text?.Trim().ToLower(); // coluna J = Vendedor
                        bool? vendedor = string.IsNullOrWhiteSpace(vendedorText) ? null : vendedorText == "sim";

                        con.Execute("sp_DataImportSallersRow_Insert", new
                        {
                            DataImportSallersLogId = logId,
                            ID                  = ws.Cells[i, 1].Text,
                            CodCliente          = ws.Cells[i, 2].Text,
                            NomeFantasia        = ws.Cells[i, 3].Text,
                            CNPJ                = ws.Cells[i, 4].Text,
                            CodProfissional     = ws.Cells[i, 5].Text,
                            Email               = ws.Cells[i, 6].Text,
                            Nome                = nome,
                            Celular             = ws.Cells[i, 8].Text,
                            CodEquipe           = ws.Cells[i, 9].Text,
                            Vendedor            = vendedor,
                            CodSuperior         = ws.Cells[i, 11].Text,
                            NomeSupervisor      = ws.Cells[i, 12].Text,
                            EmailSupervisor     = ws.Cells[i, 13].Text,
                            TelefoneSupervisor  = ws.Cells[i, 14].Text
                        }, commandType: CommandType.StoredProcedure);
                        sucessos++;
                    }
                }

                // 3. Finalizar log
                con.Execute("sp_DataImportSallersLog_Finalize", new
                {
                    DataImportSallersLogId = logId,
                    TotalRows     = totalRows,
                    ProcessedRows = sucessos,
                    ErrorRows     = erros.Count
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                if (logId > 0)
                    con.Execute("SP_Adm_DataImportSallersLog", new
                    {
                        TypeRequest = "UPDATE",
                        DataImportSallersLogId = logId,
                        Status = "ERROR",
                        ErrorMessage = ex.Message
                    }, commandType: CommandType.StoredProcedure);
                throw;
            }
            finally
            {
                con.Close();
            }

            return new ImportacaoSallersResultado { Success = sucessos, Errors = erros };
        }
    }
}
