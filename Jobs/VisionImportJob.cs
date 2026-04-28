using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class VisionImportJob
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly VisionApiConfig _apiConfig;

        public VisionImportJob(IOptions<ConnectionStrings> connectionStrings, IOptions<VisionApiConfig> apiConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _apiConfig = apiConfig.Value;
        }

        public async Task ExecutarImportacaoVisionAsync()
        {
            int dataImportProLogId = 0;
            int totalRows = 0, processedRows = 0, errorRows = 0;
            var vendedoresInseridos = new HashSet<string>();
            var supervisoresInseridos = new HashSet<string>();

            using var con = new SqlConnection(_connectionStrings.Default);
            con.Open();

            try
            {
                // 1. Criar log de importação
                var logResult = con.QueryFirstOrDefault("sp_DataImportProLog_Create", new
                {
                    FileName = $"vision-api-{DateTime.Now:yyyyMMdd-HHmmss}",
                    FileType = "API",
                    UserId = (int?)null
                }, commandType: CommandType.StoredProcedure);
                dataImportProLogId = (int)(logResult?.DataImportProLogId ?? 0);

                // 2. Consumir API com paginação
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("APIKEY", _apiConfig.ApiKey);

                int currentPage = 1;
                int totalPages = 1;

                do
                {
                    var url = $"{_apiConfig.BaseUrl}?object={_apiConfig.Object}&currentPage={currentPage}&perPage={_apiConfig.PerPage}";
                    var response = await httpClient.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<VisionApiResponse>(response);

                    if (result?.Data == null || !result.Data.Any()) break;

                    totalPages = result.Pagination?.TotalPage ?? 1;
                    totalRows += result.Data.Count;

                    foreach (var row in result.Data)
                    {
                        try
                        {
                            // Inserir vendedor único
                            var codVendedor = row.CodigoVendedor?.ToString();
                            if (!string.IsNullOrEmpty(codVendedor) && !vendedoresInseridos.Contains(codVendedor))
                            {
                                con.Execute("sp_DataImportProProfissional_Insert", new
                                {
                                    DataImportProLogId = dataImportProLogId,
                                    ID              = codVendedor,
                                    CodProfissional = codVendedor,
                                    Email           = row.EmailVendedor,
                                    Nome            = row.NomeVendedor,
                                    Celular         = row.TelefoneVendedor,
                                    Whats           = row.TelefoneVendedor,
                                    CodEquipe       = (string)null,
                                    Vendedor        = true,
                                    CodSuperior     = row.CodigoSupervisor?.ToString()
                                }, commandType: CommandType.StoredProcedure);
                                vendedoresInseridos.Add(codVendedor);
                                processedRows++;
                            }

                            // Inserir supervisor único
                            var codSupervisor = row.CodigoSupervisor?.ToString();
                            if (!string.IsNullOrEmpty(codSupervisor) && !supervisoresInseridos.Contains(codSupervisor))
                            {
                                con.Execute("sp_DataImportProProfissional_Insert", new
                                {
                                    DataImportProLogId = dataImportProLogId,
                                    ID              = codSupervisor,
                                    CodProfissional = codSupervisor,
                                    Email           = row.EmailSupervisor,
                                    Nome            = row.NomeSupervisor,
                                    Celular         = row.TelefoneSupervisor,
                                    Whats           = row.TelefoneSupervisor,
                                    CodEquipe       = (string)null,
                                    Vendedor        = false,
                                    CodSuperior     = (string)null
                                }, commandType: CommandType.StoredProcedure);
                                supervisoresInseridos.Add(codSupervisor);
                                processedRows++;
                            }

                            // Inserir cliente
                            con.Execute("sp_DataImportProCliente_Insert", new
                            {
                                DataImportProLogId = dataImportProLogId,
                                ID           = row.CodigoCliente?.ToString(),
                                CodCliente   = row.CodigoCliente?.ToString(),
                                NomeFantasia = (string)null,
                                CNPJ         = (string)null,
                                CodVendedor  = codVendedor
                            }, commandType: CommandType.StoredProcedure);
                            processedRows++;
                        }
                        catch
                        {
                            errorRows++;
                        }
                    }

                    currentPage++;

                } while (currentPage <= totalPages);

                // 3. Finalizar log
                con.Execute("sp_DataImportProLog_Finalize", new
                {
                    DataImportProLogId = dataImportProLogId,
                    TotalRows     = totalRows,
                    ProcessedRows = processedRows,
                    ErrorRows     = errorRows
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                if (dataImportProLogId > 0)
                    con.Execute("SP_Adm_DataImportProLog", new
                    {
                        TypeRequest        = "UPDATE",
                        DataImportProLogId = dataImportProLogId,
                        Status             = "ERROR",
                        ErrorMessage       = ex.Message
                    }, commandType: CommandType.StoredProcedure);
                throw;
            }
            finally
            {
                con.Close();
            }
        }
    }

    // Classes de desserialização da API
    public class VisionApiResponse
    {
        [JsonProperty("data")]
        public List<VisionApiRow> Data { get; set; }

        [JsonProperty("pagination")]
        public VisionApiPagination Pagination { get; set; }
    }

    public class VisionApiRow
    {
        [JsonProperty("codigo_vendedor")]     public int? CodigoVendedor { get; set; }
        [JsonProperty("nome_vendedor")]       public string NomeVendedor { get; set; }
        [JsonProperty("telefone_vendedor")]   public string TelefoneVendedor { get; set; }
        [JsonProperty("email_vendedor")]      public string EmailVendedor { get; set; }
        [JsonProperty("codigo_supervisor")]   public int? CodigoSupervisor { get; set; }
        [JsonProperty("nome_supervisor")]     public string NomeSupervisor { get; set; }
        [JsonProperty("telefone_supervisor")] public string TelefoneSupervisor { get; set; }
        [JsonProperty("email_supervisor")]    public string EmailSupervisor { get; set; }
        [JsonProperty("codigo_cliente")]      public int? CodigoCliente { get; set; }
    }

    public class VisionApiPagination
    {
        [JsonProperty("currentPage")] public int CurrentPage { get; set; }
        [JsonProperty("perPage")]     public int PerPage { get; set; }
        [JsonProperty("totalPage")]   public int TotalPage { get; set; }
        [JsonProperty("totalItems")]  public int TotalItems { get; set; }
    }
}
