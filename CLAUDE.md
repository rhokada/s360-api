# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Visão Geral do Projeto

**S360** é um sistema de supervisão de vendas composto por dois projetos no mesmo repositório:

- **`/` (raiz)** — API REST em ASP.NET Core 8, rodando na porta `4000`
- **`portal-supervisao-360/`** — SPA Angular 18, consome a API acima

---

## Comandos

### API (.NET)

```bash
# Build
dotnet build WebApi.csproj

# Build + restore de pacotes
dotnet restore WebApi.csproj && dotnet build WebApi.csproj

# Rodar localmente (porta 4000)
dotnet run --project WebApi.csproj

# Ao buildar com o Visual Studio aberto, use --no-restore para evitar lock de DLL
dotnet build WebApi.csproj --no-restore
```

> Não há testes automatizados no projeto .NET.

### Frontend Angular

```bash
cd portal-supervisao-360

# Dev server (porta padrão Angular, proxy para localhost:4000)
npm start          # equivale a ng serve

# Build produção
npm run build

# Build em modo watch
npm run watch
```

`environment.ts` aponta `apiUrl` para `http://localhost:4000`. Para produção, `environment.prod.ts` deve apontar para a URL do Azure.

---

## Arquitetura da API (.NET)

### Fluxo padrão de uma funcionalidade

```
Controller → Interface → Service → Stored Procedure (SQL Server via Dapper)
```

Toda lógica de negócio e acesso a dados fica nos Stored Procedures. Os Services são apenas adaptadores entre o .NET e o banco.

### Padrão de Controller

- Rota: `[Route("[controller]")]` — o nome do controller vira o path (ex: `DashIndicadoresController` → `/DashIndicadores`)
- A maioria usa `[Authorize]` (JWT Bearer)
- Serialização: usa o helper `OkDyn(dynamic obj)` com `CamelCasePropertyNamesContractResolver` do Newtonsoft.Json, em vez de retornar `Ok()` diretamente
- JWT claim do usuário: `User.FindFirst("SubjectId")?.Value`
- O raw token é passado para SPs: `Request.Headers["Authorization"].ToString().Replace("Bearer ", "")`

### Padrão de Service com colunas com acentos/espaços

Quando a SP retorna colunas com nomes problemáticos (acentos, espaços, barras como `"Tipo Questionário"`, `"N/R"`, `"NÃO"`), o mapeamento é feito via `IDictionary<string, object>` com fallbacks:

```csharp
var raw = con.Query("SP_nome", params, commandType: CommandType.StoredProcedure);
return raw.Select(r => {
    var d = (IDictionary<string, object>)r;
    return new Model {
        Campo = d.ContainsKey("Nome Coluna") ? d["Nome Coluna"]?.ToString() : d.ContainsKey("NomeColuna") ? d["NomeColuna"]?.ToString() : null
    };
}).ToList();
```

### Configurações fortemente tipadas

Cada config do `appsettings.json` tem uma classe correspondente em `Helpers/` e é registrada em `Startup.cs` com `services.Configure<T>()`. Injetada nos construtores via `IOptions<T>`.

Configs existentes: `AppSettings`, `ConnectionStrings`, `ConfigEmailBase`, `RecaptchaConfig`, `StorageConfig`, `VisionApiConfig`, `FirefliesConfig`, `OpenAiConfig`.

### Azure Blob Storage

`StorageHelpers.upload(base64, filename, storageConfig, containerName)` — método estático para upload de arquivos base64. O container principal dos áudios/imagens da S360 é `"s360"`. Arquivos temporários de processamento vão para `"s360/temp/"`.

### Hangfire Jobs

Jobs recorrentes registrados no `Startup.Configure()` via `RecurringJob.AddOrUpdate<T>()`. Cada job é uma classe com um método `async Task ExecutarAsync()`. Dashboard em `/hangfire` (auth básica: `adminhang` / `123hang`).

Jobs existentes:
- `VisionImportJob` — importa dados de API externa, roda às 03:00
- `GenerateSellerQuestionnaireEmailsJob` — roda às 08:00
- `AudioTranscriptionSubmissionJob` — submete áudios ao Fireflies.ai a cada 2h
- `AudioTranscriptionCollectionJob` — coleta transcrições prontas e gera análise GPT a cada 1h (nos :30)

### Scripts SQL

Novos scripts vão em `SqlScripts/`. Usar `IF NOT EXISTS` ao criar tabelas/colunas para idempotência. As stored procedures do sistema têm prefixo `SP_` ou `sp_`.

---

## Arquitetura do Frontend Angular

### Estrutura principal

- **`core/services/`** — services de domínio + `ApiService` (wrapper de HttpClient) + `AuthService`
- **`core/interceptors/auth.interceptor.ts`** — injeta `Authorization: Bearer {token}` em toda requisição
- **`core/guards/auth.guard.ts`** — verifica login e permissão por `slug` na rota
- **`shared/models/`** — interfaces TypeScript; cada domínio tem seu arquivo
- **`pages/`** — componentes de página, todos standalone (Angular 18)

### Autenticação e permissões

Login retorna JWT + lista de roles. O usuário escolhe uma role em `select-role`. Cada role tem `rolePermissions[]` com `slug`, `read`, `create`, `alter`, `delete`. Rotas com `data: { slug: 'nome' }` são bloqueadas pelo guard se a role não tiver permissão.

### ApiService

Todos os serviços de domínio usam `ApiService` (não `HttpClient` diretamente). Ele lê `environment.apiUrl` como base. Métodos: `get<T>`, `post<T>`, `put<T>`, `delete<T>`, `getWithParams<T>`, `postWithHeaders<T>`.

### Dashboards de indicadores (full-screen)

`indicadores` e `indicadores-notas` são dashboards estilo Power BI com filtros cruzados em memória. O padrão é:
- Cada card usa `filterRows(allRows, { ...filter, ownField: [] })` — exclui seu próprio campo do filtro para não se auto-filtrar
- Datas são formatadas com regex (sem `new Date()`) para evitar deslocamento de fuso horário
- `Chart.js` com `NgZone.run()` para change detection; `setTimeout(() => drawChart())` para aguardar o `*ngIf` renderizar o canvas
- Rotas `/indicadores` e `/indicadores-notas` têm `isLoginPage = true` no `AppComponent` para ocultar header/sidebar

### `isLoginPage` no AppComponent

Rotas que devem exibir tela cheia (sem sidebar/header) são adicionadas à condição em `app.component.ts`:
```typescript
this.isLoginPage = url.includes('/login') || url.includes('/select-role') || url.includes('/indicadores') || url.includes('/indicadores-notas');
```

---

## Conversão de Áudio (AudioTranscriptionSubmissionJob)

webm → mp3 via NAudio + LAME. Padrão obrigatório: usar **arquivo temporário em disco** para input (MediaFoundationReader não suporta webm via MemoryStream de forma confiável) e **LameMP3FileWriter com MemoryStream** para output (LAME escreve frames progressivamente, sem necessidade de seek). O WMF Sink Writer (`MediaFoundationEncoder.EncodeToMp3`) **não deve ser usado** — gera MP3 inválido.

```csharp
File.WriteAllBytes(tempInputPath, audioBytes);
using var reader = new MediaFoundationReader(tempInputPath); // decodifica webm OK
var targetFormat = new WaveFormat(44100, 16, 2);
using var resampled = new MediaFoundationResampler(reader, targetFormat);
using var outputStream = new MemoryStream();
using (var mp3Writer = new NAudio.Lame.LameMP3FileWriter(outputStream, targetFormat, 128))
{
    var buffer = new byte[targetFormat.AverageBytesPerSecond];
    int bytesRead;
    while ((bytesRead = resampled.Read(buffer, 0, buffer.Length)) > 0)
        mp3Writer.Write(buffer, 0, bytesRead);
}
```
