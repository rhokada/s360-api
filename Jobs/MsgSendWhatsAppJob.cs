using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class MsgSendWhatsAppJob
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly WhatsAppConfig _whatsAppConfig;

        public MsgSendWhatsAppJob(IOptions<ConnectionStrings> connectionStrings, IOptions<WhatsAppConfig> whatsAppConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _whatsAppConfig = whatsAppConfig.Value;
        }

        public async Task ExecutarAsync()
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            await con.OpenAsync();

            var pendentes = await con.QueryAsync<MsgToSendRow>(
                "SP_MsgGetSending",
                new { TYPE = "WHATSAPP", QTD = 50 },
                commandType: CommandType.StoredProcedure
            );

            using var httpClient = new HttpClient();

            foreach (var row in pendentes)
            {
                WhatsAppMsgData data;
                try
                {
                    data = JsonConvert.DeserializeObject<WhatsAppMsgData>(row.JsonData);
                }
                catch (Exception ex)
                {
                    await UpdateStatus(con, row.MsgToSendId, "ERRO", $"JSON inválido: {ex.Message}");
                    continue;
                }

                var phone = NormalizePhone(data?.Phone);
                if (phone == null)
                {
                    await UpdateStatus(con, row.MsgToSendId, "ERRO",
                        $"Número de telefone inválido ou ausente: '{data?.Phone}'. Esperado: 10-11 dígitos (DDD + número) ou 12-13 com código do país 55.");
                    continue;
                }

                try
                {
                    var payload = new { phone, message = data.Body };
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var url = $"{_whatsAppConfig.BaseUrl}/{_whatsAppConfig.ApiKey}";

                    var response = await httpClient.PostAsync(url, content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    WhatsAppApiResponse result;
                    try { result = JsonConvert.DeserializeObject<WhatsAppApiResponse>(responseBody); }
                    catch { result = null; }

                    if (result?.Success == true)
                        await UpdateStatus(con, row.MsgToSendId, "OK", null);
                    else
                        await UpdateStatus(con, row.MsgToSendId, "ERRO",
                            result?.Error ?? result?.Message ?? $"HTTP {(int)response.StatusCode}: {responseBody}");
                }
                catch (Exception ex)
                {
                    await UpdateStatus(con, row.MsgToSendId, "ERRO", ex.Message);
                }
            }
        }

        // Normaliza o telefone para o formato esperado pela API: "55" + DDD (2 dígitos) + número (8-9 dígitos).
        // Retorna null se o número for inválido.
        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 0) return null;

            // Já tem código do país "55" e comprimento válido (12 ou 13 dígitos)
            if (digits.StartsWith("55") && digits.Length >= 12 && digits.Length <= 13)
                return digits;

            // Sem código do país — apenas DDD + número (10 ou 11 dígitos)
            if (digits.Length >= 10 && digits.Length <= 11)
                return "55" + digits;

            return null;
        }

        private async Task UpdateStatus(SqlConnection con, int msgToSendId, string status, string errorMessage)
        {
            var notes = string.IsNullOrEmpty(errorMessage)
                ? null
                : $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {errorMessage}";

            await con.ExecuteAsync(
                @"UPDATE MsgToSend
                  SET Status = @Status,
                      DtStatus = GETDATE(),
                      Notes = CASE WHEN @Notes IS NOT NULL THEN CONCAT(@Notes, CHAR(13), CHAR(10), ISNULL(Notes, '')) ELSE Notes END
                  WHERE MsgToSendId = @MsgToSendId",
                new { MsgToSendId = msgToSendId, Status = status, Notes = notes }
            );
        }

        private class MsgToSendRow
        {
            public int MsgToSendId { get; set; }
            public string JsonData { get; set; }
        }

        private class WhatsAppMsgData
        {
            public string Phone { get; set; }
            public string Body { get; set; }
        }

        private class WhatsAppApiResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("error")]
            public string Error { get; set; }
        }
    }
}
