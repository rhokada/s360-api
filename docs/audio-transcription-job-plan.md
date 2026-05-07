# Plano de Implementação — Job de Transcrição de Áudios

## 1. Análise da API Fireflies.ai

Antes de recomendar a arquitetura, pesquisei a documentação oficial da API do Fireflies.ai.
As conclusões foram determinantes para a escolha de abordagem:

| Característica | Resultado |
|---|---|
| Protocolo | **GraphQL** — único endpoint `POST https://api.fireflies.ai/graphql` |
| Envio de áudio | **Apenas URL HTTPS pública** — sem suporte a base64 |
| Prompt customizado | **Não suportado** no upload |
| Resposta | **Assíncrona** — retorna apenas confirmação de enfileiramento |
| Notificação | **Webhook** (não há polling documentado) |
| Formatos aceitos | mp3, mp4, wav, m4a, ogg |
| **webm** | **Não documentado / não suportado oficialmente** |
| Plano mínimo | Business ou Enterprise |

### Problemas críticos com Fireflies.ai neste projeto

1. **Formato incompatível**: Os áudios gravados pelo browser são `.webm` — formato **não suportado** pelo Fireflies.
2. **Base64 não aceito**: Quando `filepath` contém base64, seria necessário primeiro converter e re-hospedar o arquivo — operação complexa.
3. **Sem prompt customizado**: A análise com o prompt solicitado exigiria uma chamada separada à API da OpenAI de qualquer forma.
4. **Assíncrono + webhook**: Em um job Hangfire, receber o retorno via webhook requer uma infraestrutura adicional (endpoint exposto, armazenamento de estado intermediário).

---

## 2. Arquitetura Recomendada

Dado que precisaríamos da OpenAI para análise de qualquer forma, e que o Whisper (OpenAI) resolve todos os problemas do Fireflies, a recomendação é:

```
┌──────────────────────────────────────────────────────────┐
│  Hangfire Job: AudioTranscriptionJob                     │
│                                                          │
│  Para cada SubmittedAnswerDetail elegível:               │
│                                                          │
│  1. Parse TimeLineJson → localiza entradas de áudio      │
│  2. Obtém o áudio:                                       │
│     ├─ Se filepath tem base64 → converte para bytes      │
│     └─ Se não → baixa da URL do Azure Blob               │
│  3. POST áudio → OpenAI Whisper API → transcript text    │
│  4. POST transcript + prompt → OpenAI Chat API → análise │
│  5. Salva AudioTranscription e AiAnalysis no banco       │
└──────────────────────────────────────────────────────────┘
```

### Por que OpenAI Whisper em vez de Fireflies

| Critério | Fireflies.ai | OpenAI Whisper |
|---|---|---|
| Aceita webm | Não | **Sim** |
| Aceita base64 / bytes | Não | **Sim** |
| Suporte a prompt | Não | N/A (só transcreve) |
| Resposta | Assíncrona (webhook) | **Síncrona** |
| Integração com ChatGPT | Extra | **Nativo (mesma SDK)** |
| Custo | Plano Business mínimo | Pay-per-use (~$0.006/min) |

---

## 3. APIs utilizadas

### 3.1 OpenAI Whisper — Transcrição

```
POST https://api.openai.com/v1/audio/transcriptions
Authorization: Bearer {OPENAI_API_KEY}
Content-Type: multipart/form-data

file: <bytes do áudio>
model: whisper-1
language: pt
response_format: verbose_json   ← retorna segmentos com timestamps
```

Resposta:
```json
{
  "text": "Texto transcrito completo...",
  "segments": [{ "start": 0.0, "end": 3.2, "text": "..." }],
  "language": "pt",
  "duration": 125.4
}
```

Essa resposta completa é salva como JSON na coluna `[AudioTranscription]`.

### 3.2 OpenAI Chat — Análise com prompt

```
POST https://api.openai.com/v1/chat/completions
Authorization: Bearer {OPENAI_API_KEY}
Content-Type: application/json

{
  "model": "gpt-4o",
  "messages": [
    {
      "role": "system",
      "content": "o texto refere-se a uma conversa entre um supervisor e um vendedor, onde o supervisor fala sobre a atuação do vendedor durante o processo de venda. extraia do texto todas as pendências do vendedor, prioridade e prazo estimado de solução. também faça um resumo da conversa identificando pontos forte e fracos."
    },
    {
      "role": "user",
      "content": "<texto transcrito pelo Whisper>"
    }
  ],
  "temperature": 0.3
}
```

Resposta completa (ou apenas `choices[0].message.content`) salva na coluna `[AiAnalysis]`.

---

## 4. Lógica de seleção dos registros

```sql
SELECT SubmittedAnswerDetailId, TimeLineJson
FROM SubmittedAnswerDetails
WHERE AudioTranscription IS NULL
  AND AiAnalysis IS NULL
  AND TimeLineJson IS NOT NULL
  AND TimeLineJson <> '[]'
  AND TimeLineJson <> ''
```

Para cada linha, o JSON é parseado em memória para encontrar itens com:
```json
{ "type": "attachment", "attachment": { "type": "audio" } }
```

Se não houver nenhum item de áudio, a linha é ignorada.

---

## 5. Obtenção do áudio (base64 vs URL)

```
Para cada entry de áudio encontrada:

├─ attachment.filepath presente e começa com "data:audio"?
│     └─ Extrai a parte base64 após a vírgula
│           Convert.FromBase64String(base64Part) → byte[]
│
└─ filepath ausente ou vazio?
      └─ HttpClient.GetByteArrayAsync(
           "https://storagebdh.blob.core.windows.net/s360/" + filename
         ) → byte[]
```

O `byte[]` resultante é enviado diretamente para o Whisper como `multipart/form-data`, usando o filename original para que a API identifique o formato (`.webm`).

---

## 6. Tratamento de erros e idempotência

- Se Whisper falhar → registra erro em log, **não salva nada** no banco (linha será reprocessada na próxima execução do job).
- Se Chat/análise falhar após transcrição bem-sucedida → salva `AudioTranscription` normalmente e deixa `AiAnalysis = NULL` para reprocessar só a análise depois.
- Processamento em lote com `try/catch` por registro: falha em um não interrompe os demais.
- Job configurado como idempotente: só processa linhas onde ambas as colunas são NULL.

---

## 7. Arquivos a criar / modificar

### Novos
| Arquivo | Descrição |
|---|---|
| `Jobs/AudioTranscriptionJob.cs` | Job principal |
| `Helpers/OpenAiConfig.cs` | Config fortemente tipada (ApiKey, modelo) |

### Modificar
| Arquivo | Mudança |
|---|---|
| `appsettings.json` | Adicionar seção `OpenAiConfig` com `ApiKey`, `WhisperModel`, `ChatModel` |
| `Startup.cs` | Registrar `AudioTranscriptionJob` no DI + `RecurringJob.AddOrUpdate` |

---

## 8. Configuração no appsettings.json

```json
"OpenAiConfig": {
  "ApiKey": "sk-...",
  "WhisperModel": "whisper-1",
  "ChatModel": "gpt-4o"
}
```

---

## 9. Agendamento sugerido

```csharp
RecurringJob.AddOrUpdate<AudioTranscriptionJob>(
    "audio-transcription",
    job => job.ExecutarAsync(),
    "0 */2 * * *"  // a cada 2 horas
);
```

Ou pode ser disparado manualmente pelo dashboard do Hangfire.

---

## 10. Dependência NuGet

Nenhuma SDK proprietária é necessária — as chamadas à OpenAI são feitas via `HttpClient` padrão do .NET com JSON serialization (`System.Text.Json`). Isso evita dependência de packages terceiros e mantém o padrão já usado no projeto.

---

## 11. Alternativa com Fireflies.ai (se já houver plano ativo)

Caso o cliente já possua um plano Business/Enterprise do Fireflies e queira usá-lo:

1. **Converter webm → mp3**: usar o package `NAudio` (.NET) para converter o áudio antes do envio.
2. **Base64 → Azure Blob**: fazer upload para o Azure Blob e gerar URL temporária com SAS token.
3. **Polling da transcrição**: após o upload, guardar o `meetingId` retornado e consultar a query `transcript` a cada N minutos até `status = "completed"`. Isso requer um segundo job ou estado intermediário no banco.
4. **Análise**: chamar a OpenAI ChatGPT de qualquer forma (Fireflies não aceita prompt customizado).

**Conclusão**: mesmo com Fireflies, ainda seria necessário a OpenAI para análise, além de conversão de formato e hospedagem intermediária. A abordagem OpenAI Whisper + ChatGPT é mais simples, mais barata e resolve todos os casos de uma vez.
