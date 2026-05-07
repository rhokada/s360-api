# Plano de Implementação — Job de Transcrição com Fireflies.ai

## 1. Visão Geral

O cliente optou pelo Fireflies.ai como serviço de transcrição. A análise com prompt customizado
será feita via OpenAI GPT-4o, pois o Fireflies não suporta prompts personalizados.

```
┌─────────────────────────────────────────────────────────────────────┐
│  JOB 1 – AudioTranscriptionSubmissionJob (a cada 2h)                │
│                                                                     │
│  Para cada SubmittedAnswerDetail sem FirefliesMeetingId:            │
│  1. Parse TimeLineJson → localiza entradas de áudio                 │
│  2. Obtém bytes do áudio (base64 ou Azure Blob)                     │
│  3. Converte webm → mp3 em memória (NAudio + MemoryStream)          │
│  4. Faz upload do mp3 convertido para Azure Blob (temp/) → URL      │
│  5. Chama mutation uploadAudio do Fireflies com a URL               │
│  6. Marca coluna FirefliesSubmittedAt com timestamp atual           │
└─────────────────────────────────────────────────────────────────────┘
              ↓ (Fireflies processa de forma assíncrona)
┌─────────────────────────────────────────────────────────────────────┐
│  WEBHOOK – POST /Fireflies/Webhook                                  │
│                                                                     │
│  Recebe: { meetingId, clientReferenceId }                           │
│  Salva meetingId em FirefliesMeetingId (chave: clientReferenceId    │
│  = SubmittedAnswerDetailId)                                         │
└─────────────────────────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────────────────────────┐
│  JOB 2 – AudioTranscriptionCollectionJob (a cada 1h)                │
│                                                                     │
│  Para cada registro com FirefliesMeetingId preenchido e sem         │
│  AudioTranscription:                                                │
│  1. Query transcript(id) no Fireflies → verifica summary_status     │
│  2. Se "completed": extrai sentences[].text → salva AudioTranscr.  │
│  3. Chama GPT-4o com transcript + prompt → salva AiAnalysis        │
│  4. Deleta arquivo temporário do Azure Blob                         │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Restrições Técnicas Confirmadas (API Fireflies.ai)

| Restrição | Detalhe |
|---|---|
| Endpoint | `POST https://api.fireflies.ai/graphql` (único endpoint GraphQL) |
| Autenticação | `Authorization: Bearer {API_KEY}` no header |
| Envio de áudio | **Apenas URL HTTPS pública** — sem base64, sem multipart |
| Formato webm | **Não suportado** — conversão para mp3 obrigatória |
| meetingId | **NÃO retornado pela mutation** — só chega via webhook |
| Polling | Possível via `transcript(id)` + campo `summary_status`, mas exige o meetingId |
| Prompt customizado | **Não suportado** — análise feita via GPT-4o separadamente |
| Plano mínimo | Business ou Enterprise |

### Sobre o upload do arquivo convertido

> A mutation `uploadAudio` do Fireflies aceita **apenas URL HTTPS pública**. Não é possível
> enviar bytes diretamente (nem base64, nem multipart/form-data).
>
> Por isso, após converter o webm para mp3 em memória, **é necessário fazer upload para o
> Azure Blob** para obter uma URL acessível. O arquivo fica em uma pasta `temp/` e é deletado
> após a coleta do transcript.
>
> Isso se aplica tanto para áudios vindos de base64 quanto para os já armazenados no Blob
> (pois estes estão em formato webm e precisam ser convertidos).

---

## 3. Alterações no Banco de Dados

Duas novas colunas na tabela `SubmittedAnswerDetails`:

```sql
ALTER TABLE SubmittedAnswerDetails
  ADD FirefliesMeetingId    NVARCHAR(100) NULL,
      FirefliesSubmittedAt  DATETIME      NULL;
```

| Coluna | Preenchida por | Significado |
|---|---|---|
| `FirefliesMeetingId` | Webhook | ID do transcript no Fireflies |
| `FirefliesSubmittedAt` | Job 1 | Quando foi submetido (evita reenvio) |
| `AudioTranscription` | Job 2 | Texto transcrito (JSON das sentences) |
| `AiAnalysis` | Job 2 | Análise do GPT-4o |

---

## 4. Fase 1 — Submissão para Fireflies

### 4.1 Seleção de registros

```sql
SELECT SubmittedAnswerDetailId, TimeLineJson
FROM SubmittedAnswerDetails
WHERE AudioTranscription IS NULL
  AND FirefliesSubmittedAt IS NULL    -- não submetido ainda
  AND TimeLineJson IS NOT NULL
  AND TimeLineJson <> '[]'
  AND TimeLineJson <> ''
```

Para cada linha, o JSON é parseado em memória para encontrar:
```json
{ "type": "attachment", "attachment": { "type": "audio" } }
```

### 4.2 Obtenção dos bytes do áudio

```
├─ attachment.filepath começa com "data:audio"?
│     └─ Extrai a parte base64 após a vírgula
│           Convert.FromBase64String(base64Part) → byte[]
│
└─ filepath ausente ou vazio?
      └─ HttpClient.GetByteArrayAsync(
           "https://storagebdh.blob.core.windows.net/s360/" + filename
         ) → byte[]
```

### 4.3 Conversão webm → mp3 em memória (NAudio)

```csharp
// Entrada: byte[] audioBytes (webm)
// Saída: byte[] mp3Bytes

using var inputStream  = new MemoryStream(audioBytes);
using var outputStream = new MemoryStream();
using var reader = new MediaFoundationReader(inputStream);  // lê webm
MediaFoundationEncoder.EncodeToMp3(reader, outputStream, 128000);
byte[] mp3Bytes = outputStream.ToArray();
```

> **Dependência**: `NAudio.Core` + `NAudio.MediaFoundation`
> A conversão usa Media Foundation do Windows (disponível no Windows Server).

### 4.4 Upload temporário para Azure Blob

O mp3 convertido é enviado para a pasta `temp/` do Azure Blob:

```
Caminho: temp/{SubmittedAnswerDetailId}_{timestamp}.mp3
URL: https://storagebdh.blob.core.windows.net/s360/temp/{arquivo}.mp3
```

A URL gerada deve ser **pública** (o container `s360` já tem acesso público, ou é necessário
gerar um SAS token de curta duração, ex: 48h).

O nome do arquivo é salvo internamente para ser deletado após a coleta do transcript.

### 4.5 Mutation uploadAudio — GraphQL

```graphql
mutation UploadAudio($input: AudioUploadInput) {
  uploadAudio(input: $input) {
    success
    title
    message
  }
}
```

Variáveis:
```json
{
  "input": {
    "url": "https://storagebdh.blob.core.windows.net/s360/temp/123_20240512.mp3",
    "title": "Supervisao-{SubmittedAnswerDetailId}",
    "custom_language": "pt",
    "client_reference_id": "123"
  }
}
```

> O campo `client_reference_id` é definido como o `SubmittedAnswerDetailId` (string).
> Ele é retornado pelo webhook e permite correlacionar o transcript com o registro no banco.

### 4.6 Atualização após submissão

```sql
UPDATE SubmittedAnswerDetails
SET FirefliesSubmittedAt = GETDATE()
WHERE SubmittedAnswerDetailId = @id
```

---

## 5. Webhook — Recebimento do meetingId

### Endpoint

```
POST /Fireflies/Webhook
Content-Type: application/json
```

### Payload recebido

```json
{
  "meetingId": "ASxwZxCstx",
  "eventType": "Transcription completed",
  "clientReferenceId": "123"
}
```

O header `x-hub-signature` contém assinatura HMAC SHA-256 para validar autenticidade.
A validação é opcional mas recomendada (configurar o `WebhookSecret` no appsettings).

### Lógica do webhook

```csharp
[HttpPost("Webhook")]
public async Task<IActionResult> Webhook([FromBody] FirefliesWebhookPayload payload)
{
    // Opcional: validar assinatura HMAC do header x-hub-signature

    var id = int.Parse(payload.ClientReferenceId);
    await _service.SaveMeetingId(id, payload.MeetingId);
    return Ok();
}
```

```sql
UPDATE SubmittedAnswerDetails
SET FirefliesMeetingId = @meetingId
WHERE SubmittedAnswerDetailId = @id
```

### Configuração do webhook no Fireflies

O webhook pode ser configurado:
- **Globalmente** no dashboard do Fireflies (app.fireflies.ai > Integrations > Webhooks)
- **Por upload** via campo `webhook` na mutation (URL para o endpoint desta API)

A abordagem **por upload** é preferida por não impactar outras integrações do cliente.

---

## 6. Fase 2 — Coleta do Transcript

### 6.1 Seleção de registros

```sql
SELECT SubmittedAnswerDetailId, FirefliesMeetingId
FROM SubmittedAnswerDetails
WHERE FirefliesMeetingId IS NOT NULL
  AND AudioTranscription IS NULL
```

### 6.2 Query transcript — GraphQL

```graphql
query Transcript($id: String!) {
  transcript(id: $id) {
    id
    title
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
}
```

Variáveis: `{ "id": "ASxwZxCstx" }`

### 6.3 Verificação de status

```
summary_status = "processing" → pular este registro (será processado na próxima execução)
summary_status = "completed"  → extrair sentences e prosseguir
```

### 6.4 Montagem do texto transcrito

As sentences são concatenadas em texto corrido e também salvas como JSON completo:

```csharp
var fullText   = string.Join(" ", sentences.Select(s => s.Text));
var jsonResult = JsonSerializer.Serialize(sentences);

// Salvar jsonResult em AudioTranscription
```

---

## 7. Análise com GPT-4o (OpenAI Chat)

```
POST https://api.openai.com/v1/chat/completions
Authorization: Bearer {OPENAI_API_KEY}
Content-Type: application/json
```

```json
{
  "model": "gpt-4o",
  "messages": [
    {
      "role": "system",
      "content": "o texto refere-se a uma conversa entre um supervisor e um vendedor, onde o supervisor fala sobre a atuação do vendedor durante o processo de venda. extraia do texto todas as pendências do vendedor, prioridade e prazo estimado de solução. também faça um resumo da conversa identificando pontos forte e fracos."
    },
    {
      "role": "user",
      "content": "<fullText extraído do Fireflies>"
    }
  ],
  "temperature": 0.3
}
```

Salvar `choices[0].message.content` na coluna `AiAnalysis`.

---

## 8. Tratamento de Erros e Idempotência

| Cenário | Comportamento |
|---|---|
| Fireflies retorna `success: false` | Log de erro; `FirefliesSubmittedAt` **não** é salvo — Job 1 reprocessa na próxima execução |
| Webhook não chega (timeout) | `FirefliesSubmittedAt` está preenchido — Job 1 não reenvia; verificação manual necessária |
| `summary_status` = "processing" | Registro ignorado no Job 2; reprocessado na próxima execução |
| GPT-4o falha após transcript salvo | `AudioTranscription` salvo; `AiAnalysis = NULL` — Job 2 reprocessa só a análise |
| Upload para Azure Blob falha | Áudio não é enviado ao Fireflies; `FirefliesSubmittedAt` **não** é salvo |
| Conversão NAudio falha | Log de erro; registro pulado sem alterar colunas |

---

## 9. Arquivos a Criar / Modificar

### Novos
| Arquivo | Descrição |
|---|---|
| `Jobs/AudioTranscriptionSubmissionJob.cs` | Job 1 — submissão para Fireflies |
| `Jobs/AudioTranscriptionCollectionJob.cs` | Job 2 — coleta do transcript + análise GPT |
| `Helpers/FirefliesConfig.cs` | Config tipada (ApiKey, WebhookSecret) |
| `Helpers/OpenAiConfig.cs` | Config tipada (ApiKey, ChatModel) |
| `Controllers/FirefliesWebhookController.cs` | Endpoint webhook |
| `Services/FirefliesWebhookService.cs` | Salva meetingId no banco |

### Modificar
| Arquivo | Mudança |
|---|---|
| `appsettings.json` | Adicionar seções `FirefliesConfig` e `OpenAiConfig` |
| `Startup.cs` | Registrar jobs no DI + `RecurringJob.AddOrUpdate` para ambos os jobs |

---

## 10. Configuração no appsettings.json

```json
"FirefliesConfig": {
  "ApiKey": "...",
  "WebhookSecret": "..."
},
"OpenAiConfig": {
  "ApiKey": "sk-...",
  "ChatModel": "gpt-4o"
},
"AzureBlobConfig": {
  "ConnectionString": "...",
  "Container": "s360",
  "TempFolder": "temp"
}
```

---

## 11. Agendamento dos Jobs

```csharp
// Job 1: submissão para Fireflies
RecurringJob.AddOrUpdate<AudioTranscriptionSubmissionJob>(
    "audio-transcription-submission",
    job => job.ExecutarAsync(),
    "0 */2 * * *"   // a cada 2 horas
);

// Job 2: coleta do transcript + análise GPT
RecurringJob.AddOrUpdate<AudioTranscriptionCollectionJob>(
    "audio-transcription-collection",
    job => job.ExecutarAsync(),
    "30 * * * *"    // a cada hora, nos :30 min (defasado do Job 1)
);
```

---

## 12. Dependências NuGet

| Package | Versão sugerida | Uso |
|---|---|---|
| `NAudio.Core` | 2.x | Leitura de webm em MemoryStream |
| `NAudio.MediaFoundation` | 2.x | Conversão para mp3 (Windows) |
| `Azure.Storage.Blobs` | 12.x | Upload temporário para Azure Blob |

As chamadas ao Fireflies e à OpenAI são feitas via `HttpClient` padrão do .NET com
`System.Text.Json` — sem SDK proprietária.

---

## 13. Fluxo de Estados de uma Linha

```
[Estado inicial]
AudioTranscription NULL | AiAnalysis NULL | FirefliesMeetingId NULL | FirefliesSubmittedAt NULL

   ↓ Job 1 executa com sucesso

AudioTranscription NULL | AiAnalysis NULL | FirefliesMeetingId NULL | FirefliesSubmittedAt = NOW()

   ↓ Fireflies processa + webhook chega

AudioTranscription NULL | AiAnalysis NULL | FirefliesMeetingId = "ASxw..." | FirefliesSubmittedAt = NOW()

   ↓ Job 2 executa com sucesso

AudioTranscription = "[{...}]" | AiAnalysis = "Análise..." | FirefliesMeetingId = "ASxw..." | FirefliesSubmittedAt = NOW()

[Estado final — linha processada]
```
