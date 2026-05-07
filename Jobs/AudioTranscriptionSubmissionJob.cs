using Azure.Storage.Blobs;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using NAudio.MediaFoundation;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class AudioTranscriptionSubmissionJob
    {
        private readonly ConnectionStrings _connectionStrings;
        private readonly StorageConfig _storageConfig;
        private readonly FirefliesConfig _firefliesConfig;

        public AudioTranscriptionSubmissionJob(
            IOptions<ConnectionStrings> connectionStrings,
            IOptions<StorageConfig> storageConfig,
            IOptions<FirefliesConfig> firefliesConfig)
        {
            _connectionStrings = connectionStrings.Value;
            _storageConfig     = storageConfig.Value;
            _firefliesConfig   = firefliesConfig.Value;
        }

        public async Task ExecutarAsync()
        {
            List<dynamic> records;

            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                con.Open();
                records = con.Query(@"
                    SELECT TOP 1 SubmittedAnswerDetailId, TimeLineJson
                    FROM   SubmittedAnswerDetails
                    WHERE  AudioTranscription  IS NULL
                      AND  FirefliesSubmittedAt IS NULL
                      AND  TimeLineJson IS NOT NULL
                      AND  TimeLineJson <> '[]'
                      AND  TimeLineJson <> ''
                ").ToList();
                con.Close();
            }

            Console.WriteLine($"[AudioTranscriptionSubmissionJob] {records.Count} registros encontrados.");

            foreach (var record in records)
            {
                int id = (int)record.SubmittedAnswerDetailId;
                string timeLineJson = (string)record.TimeLineJson;

                try
                {
                    var audioEntry = FindAudioEntry(timeLineJson);
                    if (audioEntry == null)
                    {
                        Console.WriteLine($"[AudioTranscriptionSubmissionJob] Registro {id} sem entrada de áudio, ignorado.");
                        continue;
                    }

                    var audioBytes = await GetAudioBytesAsync(audioEntry);
                    if (audioBytes == null || audioBytes.Length == 0)
                    {
                        Console.WriteLine($"[AudioTranscriptionSubmissionJob] Registro {id}: não foi possível obter bytes do áudio.");
                        continue;
                    }

                    var mp3Bytes = ConvertToMp3(audioBytes);

                    var blobPath = $"temp/{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.mp3";
                    var audioUrl = await UploadToBlobAsync(mp3Bytes, blobPath);

                    var submitted = await SubmitToFirefliesAsync(id, audioUrl);
                    if (!submitted) continue;

                    using var con = new SqlConnection(_connectionStrings.Default);
                    con.Open();
                    con.Execute(@"
                        UPDATE SubmittedAnswerDetails
                        SET    FirefliesSubmittedAt = @now
                        WHERE  SubmittedAnswerDetailId = @id
                    ", new { now = DateTime.UtcNow, id });
                    con.Close();

                    Console.WriteLine($"[AudioTranscriptionSubmissionJob] Registro {id} submetido ao Fireflies com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AudioTranscriptionSubmissionJob] Erro no registro {id}: {ex.Message}");
                }
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private TimeLineAttachment FindAudioEntry(string timeLineJson)
        {
            try
            {
                var entries = JsonConvert.DeserializeObject<List<TimeLineEntry>>(timeLineJson);
                return entries?
                    .Where(e => string.Equals(e.Type, "attachment", StringComparison.OrdinalIgnoreCase)
                             && string.Equals(e.Attachment?.Type, "audio", StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Attachment)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private async Task<byte[]> GetAudioBytesAsync(TimeLineAttachment attachment)
        {
            if (!string.IsNullOrEmpty(attachment.FilePath) &&
                attachment.FilePath.StartsWith("data:audio", StringComparison.OrdinalIgnoreCase))
            {
                var commaIndex = attachment.FilePath.IndexOf(',');
                if (commaIndex < 0) return null;
                var base64Part = attachment.FilePath.Substring(commaIndex + 1);
                return Convert.FromBase64String(base64Part);
            }

            if (!string.IsNullOrEmpty(attachment.FileName))
            {
                using var httpClient = new HttpClient();
                var blobUrl = $"https://{_storageConfig.AccountName}.blob.core.windows.net/s360/{attachment.FileName}";
                return await httpClient.GetByteArrayAsync(blobUrl);
            }

            return null;
        }

        private byte[] ConvertToMp3(byte[] audioBytes)
        {
            // MediaFoundationReader precisa de um arquivo em disco; a saída é via MemoryStream.
            // O suporte a webm depende dos codecs instalados no Windows (Media Foundation).
            var tempInputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.webm");
            try
            {
                File.WriteAllBytes(tempInputPath, audioBytes);
                using var reader       = new MediaFoundationReader(tempInputPath);
                using var outputStream = new MemoryStream();
                MediaFoundationEncoder.EncodeToMp3(reader, outputStream, 128000);
                return outputStream.ToArray();
            }
            finally
            {
                if (File.Exists(tempInputPath)) File.Delete(tempInputPath);
            }
        }

        private async Task<string> UploadToBlobAsync(byte[] mp3Bytes, string blobPath)
        {
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_storageConfig.AccountName};AccountKey={_storageConfig.AccountKey};EndpointSuffix=core.windows.net";
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient   = blobServiceClient.GetBlobContainerClient("s360");
            var blobClient        = containerClient.GetBlobClient(blobPath);

            using var stream = new MemoryStream(mp3Bytes);
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        private async Task<bool> SubmitToFirefliesAsync(int recordId, string audioUrl)
        {
            const string endpoint = "https://api.fireflies.ai/graphql";
            const string mutation = @"mutation UploadAudio($input: AudioUploadInput) {
  uploadAudio(input: $input) {
    success
    title
    message
  }
}";
            var payload = new
            {
                query     = mutation,
                variables = new
                {
                    input = new
                    {
                        url                  = audioUrl,
                        title                = $"Supervisao-{recordId}",
                        custom_language      = "pt",
                        client_reference_id  = recordId.ToString(),
                        webhook              = _firefliesConfig.WebhookUrl
                    }
                }
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _firefliesConfig.ApiKey);

            var content  = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            var body     = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<FirefliesUploadResponse>(body);
            if (result?.Data?.UploadAudio?.Success == true)
                return true;

            Console.WriteLine($"[AudioTranscriptionSubmissionJob] Fireflies rejeitou registro {recordId}: {body}");
            return false;
        }
    }

    // ─── Modelos TimeLineJson ─────────────────────────────────────────────────

    public class TimeLineEntry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("attachment")]
        public TimeLineAttachment Attachment { get; set; }
    }

    public class TimeLineAttachment
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("filepath")]
        public string FilePath { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }
    }

    // ─── Modelos resposta Fireflies uploadAudio ───────────────────────────────

    public class FirefliesUploadResponse
    {
        [JsonProperty("data")]
        public FirefliesUploadData Data { get; set; }
    }

    public class FirefliesUploadData
    {
        [JsonProperty("uploadAudio")]
        public FirefliesUploadResult UploadAudio { get; set; }
    }

    public class FirefliesUploadResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
