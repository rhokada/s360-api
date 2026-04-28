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
            int logId = 0;
            int totalRows = 0, processedRows = 0, errorRows = 0;

            using var con = new SqlConnection(_connectionStrings.Default);
            con.Open();

            try
            {
                // 1. Criar log de importação
                var logResult = con.QueryFirstOrDefault("sp_DataImportSallersLog_Create", new
                {
                    FileName = $"vision-api-{DateTime.Now:yyyyMMdd-HHmmss}",
                    UserId   = (int?)null
                }, commandType: CommandType.StoredProcedure);
                logId = (int)(logResult?.DataImportSallersLogId ?? 0);

                // 2. Consumir API com paginação
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("APIKEY", _apiConfig.ApiKey);

                int currentPage = 1;
                int totalPages  = 1;

                do
                {
                    var url      = $"{_apiConfig.BaseUrl}?object={_apiConfig.Object}&currentPage={currentPage}&perPage={_apiConfig.PerPage}";
                    var response = await httpClient.GetStringAsync(url);
                    var result   = JsonConvert.DeserializeObject<VisionApiResponse>(response);

                    if (result?.Data == null || !result.Data.Any()) break;

                    totalPages = result.Pagination?.TotalPage ?? 1;
                    totalRows += result.Data.Count;

                    foreach (var row in result.Data)
                    {
                        try
                        {
                            // Cada linha da API vira uma linha em DataImportSallersRow
                            // combinando os dados do vendedor, supervisor e cliente
                            con.Execute("sp_DataImportSallersRow_Insert", new
                            {
                                DataImportSallersLogId = logId,
                                ID                  = row.CodigoVendedor?.ToString(),
                                CodCliente          = row.CodigoCliente?.ToString(),
                                NomeFantasia        = (string)null,
                                CNPJ                = (string)null,
                                CodProfissional     = row.CodigoVendedor?.ToString(),
                                Email               = row.EmailVendedor,
                                Nome                = row.NomeVendedor,
                                Celular             = row.TelefoneVendedor,
                                CodEquipe           = (string)null,
                                Vendedor            = true,
                                CodSuperior         = row.CodigoSupervisor?.ToString(),
                                row.NomeSupervisor,
                                row.TelefoneSupervisor,
                                row.EmailSupervisor
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
                con.Execute("sp_DataImportSallersLog_Finalize", new
                {
                    DataImportSallersLogId = logId,
                    TotalRows              = totalRows,
                    ProcessedRows          = processedRows,
                    ErrorRows              = errorRows
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                if (logId > 0)
                    con.Execute("SP_Adm_DataImportSallersLog", new
                    {
                        TypeRequest              = "UPDATE",
                        DataImportSallersLogId   = logId,
                        Status                   = "ERROR",
                        ErrorMessage             = ex.Message
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
