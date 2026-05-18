using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class MsgSendEmailJob
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly ConfigEmailBase _emailConfig;

        public MsgSendEmailJob(IOptions<ConnectionStrings> connectionStrings, IOptions<ConfigEmailBase> emailConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _emailConfig = emailConfig.Value;
        }

        public async Task ExecutarAsync()
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            await con.OpenAsync();

            var pendentes = await con.QueryAsync<MsgToSendRow>(
                "SP_MsgGetSending",
                new { TYPE = "EMAIL", QTD = 50 },
                commandType: CommandType.StoredProcedure
            );

            foreach (var row in pendentes)
            {
                EmailMsgData data;
                try
                {
                    data = JsonConvert.DeserializeObject<EmailMsgData>(row.JsonData);
                }
                catch (Exception ex)
                {
                    await UpdateStatus(con, row.MsgToSendId, "ERRO", $"JSON inválido: {ex.Message}");
                    continue;
                }

                try
                {
                    Mail.Send(
                        toAddress: data.To,
                        toName: data.ToName ?? "",
                        ccAddress: data.Cc ?? "",
                        ccName: data.CcName ?? "",
                        subject: data.Subject,
                        body: data.Body,
                        infos: _emailConfig
                    );

                    await UpdateStatus(con, row.MsgToSendId, "OK", null);
                }
                catch (Exception ex)
                {
                    await UpdateStatus(con, row.MsgToSendId, "ERRO", ex.Message);
                }
            }
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

        private class EmailMsgData
        {
            public string To { get; set; }
            public string ToName { get; set; }
            public string Cc { get; set; }
            public string CcName { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}
