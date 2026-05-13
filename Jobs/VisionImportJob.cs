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
        private readonly VisionApiConfig   _apiConfig;

        public VisionImportJob(IOptions<ConnectionStrings> connectionStrings, IOptions<VisionApiConfig> apiConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _apiConfig         = apiConfig.Value;
        }

        public async Task ExecutarImportacaoVisionAsync()
        {
            int logId         = 0;
            int totalRows     = 0;
            int processedRows = 0;
            int errorRows     = 0;

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

                // 2. Consumir API com paginação e bulk insert por página
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var httpClient = new HttpClient(handler);
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

                    try
                    {
                        var table = BuildDataTable(logId, result.Data);

                        using var bulk = new SqlBulkCopy(con)
                        {
                            DestinationTableName = "DataImportSallersRow",
                            BatchSize            = 1000
                        };
                        MapBulkColumns(bulk);
                        await bulk.WriteToServerAsync(table);

                        processedRows += result.Data.Count;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[VisionImportJob] Erro no bulk insert da página {currentPage}: {ex.Message}");
                        errorRows += result.Data.Count;
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
                        TypeRequest            = "UPDATE",
                        DataImportSallersLogId = logId,
                        Status                 = "ERROR",
                        ErrorMessage           = ex.Message
                    }, commandType: CommandType.StoredProcedure);
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        private static DataTable BuildDataTable(int logId, List<VisionApiRow> rows)
        {
            var table = new DataTable();
            table.Columns.Add("DataImportSallersLogId", typeof(int));
            table.Columns.Add("ID",                     typeof(string));
            table.Columns.Add("CodCliente",             typeof(string));
            table.Columns.Add("NomeFantasia",           typeof(string));
            table.Columns.Add("CNPJ",                   typeof(string));
            table.Columns.Add("CodProfissional",        typeof(string));
            table.Columns.Add("Email",                  typeof(string));
            table.Columns.Add("Nome",                   typeof(string));
            table.Columns.Add("Celular",                typeof(string));
            table.Columns.Add("CodEquipe",              typeof(string));
            table.Columns.Add("Vendedor",               typeof(bool));
            table.Columns.Add("CodSuperior",            typeof(string));
            table.Columns.Add("NomeSupervisor",         typeof(string));
            table.Columns.Add("TelefoneSupervisor",     typeof(string));
            table.Columns.Add("EmailSupervisor",        typeof(string));
            table.Columns.Add("Status",                 typeof(string));
            table.Columns.Add("DhCreate",               typeof(DateTime));

            var now = DateTime.Now;
            foreach (var row in rows)
            {
                table.Rows.Add(
                    logId,
                    row.CodigoVendedor?.ToString(),
                    row.CodigoCliente?.ToString(),
                    row.Fantasia,
                    DBNull.Value,
                    row.CodigoVendedor?.ToString(),
                    row.EmailVendedor,
                    row.NomeVendedor,
                    row.TelefoneVendedor,
                    DBNull.Value,
                    true,
                    row.CodigoSupervisor?.ToString(),
                    row.NomeSupervisor,
                    row.TelefoneSupervisor,
                    row.EmailSupervisor,
                    "PENDING",
                    now
                );
            }

            return table;
        }

        private static void MapBulkColumns(SqlBulkCopy bulk)
        {
            foreach (var col in new[]
            {
                "DataImportSallersLogId", "ID", "CodCliente", "NomeFantasia", "CNPJ",
                "CodProfissional", "Email", "Nome", "Celular", "CodEquipe", "Vendedor",
                "CodSuperior", "NomeSupervisor", "TelefoneSupervisor", "EmailSupervisor",
                "Status", "DhCreate"
            })
            {
                bulk.ColumnMappings.Add(col, col);
            }
        }
    }

    public class VisionApiResponse
    {
        [JsonProperty("data")]
        public List<VisionApiRow> Data { get; set; }

        [JsonProperty("pagination")]
        public VisionApiPagination Pagination { get; set; }
    }

    public class VisionApiRow
    {
        [JsonProperty("codigo_vendedor")]     public int?   CodigoVendedor     { get; set; }
        [JsonProperty("nome_vendedor")]       public string NomeVendedor        { get; set; }
        [JsonProperty("telefone_vendedor")]   public string TelefoneVendedor    { get; set; }
        [JsonProperty("email_vendedor")]      public string EmailVendedor        { get; set; }
        [JsonProperty("codigo_supervisor")]   public int?   CodigoSupervisor    { get; set; }
        [JsonProperty("nome_supervisor")]     public string NomeSupervisor       { get; set; }
        [JsonProperty("telefone_supervisor")] public string TelefoneSupervisor   { get; set; }
        [JsonProperty("email_supervisor")]    public string EmailSupervisor       { get; set; }
        [JsonProperty("codigo_cliente")]      public int?   CodigoCliente        { get; set; }
        [JsonProperty("fantasia")]            public string Fantasia              { get; set; }
    }

    public class VisionApiPagination
    {
        [JsonProperty("currentPage")] public int CurrentPage { get; set; }
        [JsonProperty("perPage")]     public int PerPage     { get; set; }
        [JsonProperty("totalPage")]   public int TotalPage   { get; set; }
        [JsonProperty("totalItems")]  public int TotalItems  { get; set; }
    }
}
