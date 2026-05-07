using Azure.Storage.Blobs;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class AudioTranscriptionCollectionJob
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly StorageConfig     _storageConfig;
        private readonly FirefliesConfig   _firefliesConfig;
        private readonly OpenAiConfig      _openAiConfig;

        public AudioTranscriptionCollectionJob(
            IOptions<ConnectionStrings> connectionStrings,
            IOptions<StorageConfig>     storageConfig,
            IOptions<FirefliesConfig>   firefliesConfig,
            IOptions<OpenAiConfig>      openAiConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _storageConfig     = storageConfig.Value;
            _firefliesConfig   = firefliesConfig.Value;
            _openAiConfig      = openAiConfig.Value;
        }

        public async Task ExecutarAsync()
        {
            List<dynamic> records;

            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                con.Open();
                records = con.Query(@"
                    SELECT SubmittedAnswerDetailId, FirefliesMeetingId
                    FROM   SubmittedAnswerDetails
                    WHERE  FirefliesMeetingId IS NOT NULL
                      AND  AudioTranscription IS NULL
                ").ToList();
                con.Close();
            }

            Console.WriteLine($"[AudioTranscriptionCollectionJob] {records.Count} registros aguardando coleta.");

            foreach (var record in records)
            {
                int    id        = (int)record.SubmittedAnswerDetailId;
                string meetingId = (string)record.FirefliesMeetingId;

                try
                {
                    var (status, sentences) = await GetFirefliesTranscriptAsync(meetingId);

                    if (status != "completed" && status != "processed")
                    {
                        Console.WriteLine($"[AudioTranscriptionCollectionJob] Registro {id}: status '{status}', aguardando próxima execução.");
                        continue;
                    }

                    var transcriptJson = JsonConvert.SerializeObject(sentences);
                    var fullText       = string.Join(" ", sentences.Select(s => s.Text));

                    // Salva o transcript
                    using (var con = new SqlConnection(_connectionStrings.Default))
                    {
                        con.Open();
                        con.Execute(@"
                            UPDATE SubmittedAnswerDetails
                            SET    AudioTranscription = @transcript
                            WHERE  SubmittedAnswerDetailId = @id
                        ", new { transcript = transcriptJson, id });
                        con.Close();
                    }

                    // Análise com GPT-4o
                    var analysis = await GetGptAnalysisAsync(transcriptJson);

                    if (!string.IsNullOrEmpty(analysis))
                    {
                        using var con = new SqlConnection(_connectionStrings.Default);
                        con.Open();
                        con.Execute(@"
                            UPDATE SubmittedAnswerDetails
                            SET    AiAnalysis = @analysis
                            WHERE  SubmittedAnswerDetailId = @id
                        ", new { analysis, id });
                        con.Close();
                    }

                    // Remove o arquivo temporário do Azure Blob
                    await DeleteTempBlobAsync(id);

                    Console.WriteLine($"[AudioTranscriptionCollectionJob] Registro {id} processado com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AudioTranscriptionCollectionJob] Erro no registro {id}: {ex.Message}");
                }
            }
        }

        // ─── Fireflies ────────────────────────────────────────────────────────

        private async Task<(string Status, List<FirefliesSentence> Sentences)> GetFirefliesTranscriptAsync(string meetingId)
        {
            const string endpoint = "https://api.fireflies.ai/graphql";
            const string query    = @"query Transcript($id: String!) {
  transcript(id: $id) {
    id
    meeting_info {
      summary_status
    }
    sentences {
      text
      speaker_name
      start_time
      end_time
    }
  }
}";
            var payload = new { query, variables = new { id = meetingId } };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _firefliesConfig.ApiKey);

            var content  = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            var body     = await response.Content.ReadAsStringAsync();

            var result     = JsonConvert.DeserializeObject<FirefliesTranscriptResponse>(body);
            var transcript = result?.Data?.Transcript;

            if (transcript == null)
                return ("unknown", new List<FirefliesSentence>());

            var status    = transcript.MeetingInfo?.SummaryStatus ?? "unknown";
            var sentences = transcript.Sentences ?? new List<FirefliesSentence>();
            return (status, sentences);
        }

        // ─── OpenAI GPT-4o ────────────────────────────────────────────────────

        private async Task<string> GetGptAnalysisAsync(string transcriptText)
        {
            const string endpoint     = "https://api.openai.com/v1/chat/completions";
            const string systemPrompt = @"o texto refere-se a uma conversa entre um supervisor e um vendedor, onde o supervisor fala sobre a atuação do vendedor durante o processo de venda. extraia do texto todas as pendências do vendedor, prioridade e prazo estimado de solução. também faça um resumo da conversa identificando pontos forte e fracos.
               

                Retorne APENAS um JSON válido.
                Não escreva explicações.
                Não utilize markdown.
                Não utilize texto antes ou depois do JSON.

                Formato obrigatório:
                {
                    ""resumo"": ""string"",
                    ""pontos_fortes"": ""string"",
                    ""pontos_fracos"": ""string"",
                    ""pendencias"": [
                      {
                        ""pendencia"": ""string"",
                        ""prioridade"": ""string"",
                        ""prazo_estimado"": ""string""
                      }
                    ]
                }
            ";

            var payload = new
            {
                model       = _openAiConfig.ChatModel,
                messages    = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = transcriptText }
                },
                verbosity = "medium",
                reasoning_effort = "medium",
                store = false
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiConfig.ApiKey);

            var content  = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            var body     = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<OpenAiChatResponse>(body);
            return result?.Choices?.FirstOrDefault()?.Message?.Content;
        }

        // ─── Azure Blob temp cleanup ──────────────────────────────────────────

        private async Task DeleteTempBlobAsync(int recordId)
        {
            try
            {
                var connectionString  = $"DefaultEndpointsProtocol=https;AccountName={_storageConfig.AccountName};AccountKey={_storageConfig.AccountKey};EndpointSuffix=core.windows.net";
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient   = blobServiceClient.GetBlobContainerClient("s360");

                await foreach (var blob in containerClient.GetBlobsAsync(prefix: $"temp/{recordId}_"))
                    await containerClient.GetBlobClient(blob.Name).DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioTranscriptionCollectionJob] Aviso: falha ao deletar blob temp do registro {recordId}: {ex.Message}");
            }
        }
    }

    // ─── Modelos resposta Fireflies transcript ────────────────────────────────

    public class FirefliesTranscriptResponse
    {
        [JsonProperty("data")]
        public FirefliesTranscriptData Data { get; set; }
    }

    public class FirefliesTranscriptData
    {
        [JsonProperty("transcript")]
        public FirefliesTranscript Transcript { get; set; }
    }

    public class FirefliesTranscript
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("meeting_info")]
        public FirefliesMeetingInfo MeetingInfo { get; set; }

        [JsonProperty("sentences")]
        public List<FirefliesSentence> Sentences { get; set; }
    }

    public class FirefliesMeetingInfo
    {
        [JsonProperty("summary_status")]
        public string SummaryStatus { get; set; }
    }

    public class FirefliesSentence
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("speaker_name")]
        public string SpeakerName { get; set; }

        [JsonProperty("start_time")]
        public double StartTime { get; set; }

        [JsonProperty("end_time")]
        public double EndTime { get; set; }
    }

    // ─── Modelos resposta OpenAI Chat ─────────────────────────────────────────

    public class OpenAiChatResponse
    {
        [JsonProperty("choices")]
        public List<OpenAiChoice> Choices { get; set; }
    }

    public class OpenAiChoice
    {
        [JsonProperty("message")]
        public OpenAiMessage Message { get; set; }
    }

    public class OpenAiMessage
    {
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
