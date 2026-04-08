# Guia de Migração: .NET Core 3.0 → .NET 8

## Resumo

A migração é **viável**, porém envolve esforço moderado devido ao salto de versão (3.0 → 8.0) e à presença de **dois pacotes deprecados** que exigem substituição por novas bibliotecas com APIs diferentes.

---

## Diagnóstico Atual

| Item | Situação Atual |
|------|---------------|
| Framework | `netcoreapp3.0` |
| Pacotes NuGet | 9 dependências |
| Pacotes Deprecados | 2 (`WindowsAzure.Storage`, `System.Data.SqlClient`) |
| Pacotes que precisam de atualização de versão | 4 |
| Testes automatizados | Nenhum |

---

## Passo 1 — Atualizar o TargetFramework

**Arquivo:** [WebApi.csproj](WebApi.csproj)

```xml
<!-- Antes -->
<TargetFramework>netcoreapp3.0</TargetFramework>

<!-- Depois -->
<TargetFramework>net8.0</TargetFramework>
```

---

## Passo 2 — Atualizar Pacotes NuGet

### 2.1 Pacotes com atualização simples de versão

Esses pacotes continuam compatíveis com .NET 8, basta atualizar a versão:

| Pacote | Versão Atual | Versão para .NET 8 |
|--------|-------------|---------------------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 3.0.0 | **8.0.x** |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | 3.0.0 | **8.0.x** |
| `System.IdentityModel.Tokens.Jwt` | 5.6.0 | **8.x** |
| `Dapper` | 2.0.30 | **2.1.x** |
| `MailKit` | 2.5.0 | **4.x** |
| `Newtonsoft.Json` | 12.0.3 | **13.x** |
| `Google.Apis.Vision.v1` | 1.43.0.1835 | **Versão mais recente** |

> **Nota:** Os pacotes `Microsoft.AspNetCore.*` **devem** ter a versão alinhada com o framework alvo. Usar versão 3.0.0 com `net8.0` causará erros de compilação.

### 2.2 Pacotes Deprecados — Requerem Substituição

#### `WindowsAzure.Storage` 9.3.3 → **DEPRECADO**

Este pacote foi descontinuado pela Microsoft. A substituição é o SDK moderno do Azure:

| Funcionalidade | Pacote Antigo | Pacote Novo |
|---------------|--------------|-------------|
| Blob Storage | `WindowsAzure.Storage` | `Azure.Storage.Blobs` |
| Queue Storage | `WindowsAzure.Storage` | `Azure.Storage.Queues` |

**Impacto no código:** A API é completamente diferente. Todo código que usa `CloudStorageAccount`, `CloudBlobClient`, `CloudBlobContainer` e `CloudBlockBlob` precisará ser reescrito.

Arquivos impactados:
- [Helpers/StorageConfig.cs](Helpers/StorageConfig.cs)
- Qualquer service que faça upload de arquivos

**Exemplo de mudança de API:**

```csharp
// Antes (WindowsAzure.Storage)
var storageAccount = CloudStorageAccount.Parse(connectionString);
var blobClient = storageAccount.CreateCloudBlobClient();
var container = blobClient.GetContainerReference("meu-container");
var blob = container.GetBlockBlobReference("arquivo.jpg");
await blob.UploadFromStreamAsync(stream);

// Depois (Azure.Storage.Blobs)
var blobServiceClient = new BlobServiceClient(connectionString);
var containerClient = blobServiceClient.GetBlobContainerClient("meu-container");
var blobClient = containerClient.GetBlobClient("arquivo.jpg");
await blobClient.UploadAsync(stream);
```

---

#### `System.Data.SqlClient` 4.7.0 → **LEGADO**

A Microsoft criou um pacote sucessor com suporte ativo:

| Substituto | Pacote | Observação |
|-----------|--------|-----------|
| `Microsoft.Data.SqlClient` | `Microsoft.Data.SqlClient` 5.x | Mantido ativamente |

**Impacto no código:** O namespace muda de `System.Data.SqlClient` para `Microsoft.Data.SqlClient`. A API principal é a mesma (mesmos nomes de classe como `SqlConnection`, `SqlCommand`, etc.), então a migração é basicamente uma troca de `using`.

Arquivos impactados:
- [Helpers/ConnectionStrings.cs](Helpers/ConnectionStrings.cs)
- Todos os Services que fazem acesso a banco de dados

```csharp
// Antes
using System.Data.SqlClient;

// Depois
using Microsoft.Data.SqlClient;
```

---

## Passo 3 — Atualizar Program.cs e Startup.cs

O .NET Core 3.0 usa o modelo `WebHost.CreateDefaultBuilder` com `Startup.cs`. No .NET 8, o padrão recomendado é o **Minimal Hosting Model** (introduzido no .NET 6).

**Opção A — Manter o padrão antigo (menos esforço)**

O modelo com `Startup.cs` ainda é suportado no .NET 8. Basta ajustar o `Program.cs`:

```csharp
// Antes (Program.cs .NET Core 3.0)
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://localhost:4000");
            });
}

// Depois (Program.cs .NET 8 mantendo Startup.cs)
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:4000");

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();
```

**Opção B — Migrar para Minimal Hosting Model (recomendado)**

Consolidar `Program.cs` e `Startup.cs` em um único arquivo `Program.cs`. Mais trabalho inicial, mas alinhado com as práticas modernas do .NET.

---

## Passo 4 — Verificar Breaking Changes Relevantes

### 4.1 Nullable Reference Types

O .NET 8 habilita por padrão avisos de nullable reference types. Pode gerar um volume alto de warnings no código. Para desabilitar inicialmente:

```xml
<!-- WebApi.csproj -->
<Nullable>disable</Nullable>
```

### 4.2 `IWebHostEnvironment` vs `IHostingEnvironment`

O `IHostingEnvironment` foi removido no .NET 5+. Se estiver sendo usado no `Startup.cs`, deve ser substituído:

```csharp
// Antes
using Microsoft.AspNetCore.Hosting;
IHostingEnvironment env

// Depois
using Microsoft.Extensions.Hosting;
IWebHostEnvironment env
```

### 4.3 `Endpoint Routing`

O roteamento por endpoint já era obrigatório no 3.0, então não há mudanças aqui.

### 4.4 JSON padrão

O .NET 8 usa `System.Text.Json` por padrão, mas o projeto já usa `Newtonsoft.Json` via `AddNewtonsoftJson()`. Isso continua funcionando normalmente após a atualização dos pacotes.

---

## Passo 5 — Atualizar SDK no dotnet-tools.json (se aplicável)

Verificar se o arquivo `.config/dotnet-tools.json` referencia ferramentas compatíveis com .NET 8.

---

## Ordem de Execução Sugerida

```
1. Fazer backup / criar branch de migração
2. Atualizar TargetFramework para net8.0
3. Atualizar pacotes Microsoft.AspNetCore.* para 8.0.x
4. Atualizar demais pacotes (Dapper, MailKit, etc.)
5. Substituir System.Data.SqlClient → Microsoft.Data.SqlClient
6. Substituir WindowsAzure.Storage → Azure.Storage.Blobs
7. Ajustar usings e código impactado pela mudança de API do Storage
8. Verificar e corrigir erros de compilação
9. Testar todos os endpoints manualmente
10. (Opcional) Migrar para Minimal Hosting Model
```

---

## Estimativa de Risco por Componente

| Componente | Risco | Motivo |
|-----------|-------|--------|
| Autenticação JWT | Baixo | Apenas atualização de versão do pacote |
| Banco de dados (Dapper + SQL) | Baixo | Troca de namespace apenas |
| Envio de email (MailKit) | Baixo | API compatível entre versões |
| Google Vision API | Baixo | Apenas atualização de versão |
| Azure Storage | **Alto** | API completamente diferente, reescrita necessária |
| Program.cs / Startup.cs | Médio | Ajuste de estrutura do host builder |

---

## Conclusão

A migração é possível e recomendada, pois o .NET Core 3.0 **está fora de suporte desde março de 2020** e o .NET 8 é uma versão LTS (Long Term Support) com suporte até novembro de 2026.

O principal ponto de atenção é a **substituição do `WindowsAzure.Storage`**, que exige reescrita do código de integração com o Azure Blob Storage. O restante da migração é incremental e de baixo risco.
