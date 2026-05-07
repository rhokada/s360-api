using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FirefliesWebhookController : ControllerBase
    {
        private readonly ConnectionStrings _connectionStrings;

        public FirefliesWebhookController(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        // Endpoint chamado pelo Fireflies quando a transcrição é concluída.
        // Não requer autenticação JWT — é um callback externo.
        [HttpPost]
        public IActionResult Receive([FromBody] FirefliesWebhookPayload payload)
        {
            if (string.IsNullOrEmpty(payload?.MeetingId) || string.IsNullOrEmpty(payload?.ClientReferenceId))
                return BadRequest();

            if (!int.TryParse(payload.ClientReferenceId, out var recordId))
                return BadRequest();

            using var con = new SqlConnection(_connectionStrings.Default);
            con.Open();
            con.Execute(@"
                UPDATE SubmittedAnswerDetails
                SET    FirefliesMeetingId = @meetingId
                WHERE  SubmittedAnswerDetailId = @id
            ", new { meetingId = payload.MeetingId, id = recordId });
            con.Close();

            Console.WriteLine($"[FirefliesWebhook] MeetingId '{payload.MeetingId}' salvo para registro {recordId}.");
            return Ok();
        }
    }

    public class FirefliesWebhookPayload
    {
        [JsonProperty("meetingId")]
        public string MeetingId { get; set; }

        [JsonProperty("clientReferenceId")]
        public string ClientReferenceId { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }
    }
}
