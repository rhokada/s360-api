# Backend — Endpoint de Importação de Planilha

## Visão geral

O frontend envia um arquivo `.xlsx` via `multipart/form-data`. O backend lê as duas abas (`Profissionais` e `Clientes`), salva os dados brutos nas tabelas de staging e retorna um sumário. O processamento posterior dos dados fica a cargo de outra equipe/etapa.

---

## Tabelas de banco

As tabelas foram criadas pelo script `scripts/create-tables-importacao.sql`.

### `DataImportPro` — Cabeçalho de cada importação
| Coluna | Tipo | Descrição |
|---|---|---|
| DataImportProId | INT IDENTITY | PK |
| FileName | VARCHAR(200) | Nome do arquivo enviado |
| FileType | VARCHAR(20) | Fixo: `AMBOS` |
| Status | VARCHAR(20) | `PROCESSING` → `COMPLETED` \| `ERROR` |
| TotalRows | INT | Total de linhas lidas (profissionais + clientes) |
| ProcessedRows | INT | Linhas inseridas com sucesso |
| ErrorRows | INT | Linhas ignoradas por erro |
| UserId | INT | Id do usuário que fez o upload (opcional) |
| DhCreate | DATETIME | Data do upload |

### `DataImportProProfissional` — Dados brutos da aba Profissionais
| Coluna | Tipo |
|---|---|
| DataImportProProfissionalId | INT IDENTITY (PK) |
| DataImportProId | INT (FK → DataImportPro) |
| ID | VARCHAR(50) |
| CodProfissional | VARCHAR(50) |
| Email | VARCHAR(200) |
| Nome | VARCHAR(200) |
| Celular | VARCHAR(20) |
| Whats | VARCHAR(20) |
| CodEquipe | VARCHAR(50) |
| Vendedor | BIT |
| CodSuperior | VARCHAR(50) |
| Status | VARCHAR(20) | `PENDING` (padrão ao inserir) |
| ErrorMessage | NVARCHAR(MAX) |
| DhCreate | DATETIME |

### `DataImportProCliente` — Dados brutos da aba Clientes
| Coluna | Tipo |
|---|---|
| DataImportProClienteId | INT IDENTITY (PK) |
| DataImportProId | INT (FK → DataImportPro) |
| ID | VARCHAR(50) |
| CodCliente | VARCHAR(50) |
| NomeFantasia | VARCHAR(200) |
| CNPJ | VARCHAR(20) |
| CodVendedor | VARCHAR(50) |
| Status | VARCHAR(20) | `PENDING` (padrão ao inserir) |
| ErrorMessage | NVARCHAR(MAX) |
| DhCreate | DATETIME |

---

## Endpoint

| Método | URL | Auth | Content-Type |
|---|---|---|---|
| POST | `/DataImportPro/ImportarPlanilha` | Bearer JWT | `multipart/form-data` |

Campo do form: `file` (IFormFile)

**Resposta:**
```json
{
  "success": 42,
  "errors": [
    "Linha 5 (Profissionais): Nome é obrigatório.",
    "Linha 3 (Clientes): NomeFantasia é obrigatório."
  ]
}
```

---

## Controller (.NET)

```csharp
[HttpPost("ImportarPlanilha")]
[Authorize]
public async Task<IActionResult> ImportarPlanilha(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest(new { message = "Nenhum arquivo recebido." });

    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        return BadRequest(new { message = "Formato inválido. Envie um arquivo .xlsx." });

    using var stream = file.OpenReadStream();
    var resultado = await _importacaoService.ImportarAsync(stream, file.FileName);

    return Ok(resultado);
}
```

---

## Service de importação

Responsabilidade: ler o Excel, inserir nas tabelas de staging e retornar o sumário. Nada além disso.

```csharp
public async Task<ImportacaoResultado> ImportarAsync(Stream stream, string fileName)
{
    var erros = new List<string>();

    // 1. Criar registro de controle
    var dataImportId = await _repo.CriarDataImportProAsync(fileName);

    // 2. Ler e inserir profissionais
    var profRows = LerProfissionais(stream);
    foreach (var (row, linha) in profRows.Select((r, i) => (r, i + 2)))
    {
        if (string.IsNullOrWhiteSpace(row.Nome))
        {
            erros.Add($"Linha {linha} (Profissionais): Nome é obrigatório.");
            continue;
        }
        await _repo.InserirProfissionalAsync(dataImportId, row);
    }

    // 3. Ler e inserir clientes
    var cliRows = LerClientes(stream);
    foreach (var (row, linha) in cliRows.Select((r, i) => (r, i + 2)))
    {
        if (string.IsNullOrWhiteSpace(row.NomeFantasia))
        {
            erros.Add($"Linha {linha} (Clientes): NomeFantasia é obrigatório.");
            continue;
        }
        await _repo.InserirClienteAsync(dataImportId, row);
    }

    // 4. Atualizar status do DataImportPro
    var total = profRows.Count + cliRows.Count;
    var sucesso = total - erros.Count;
    await _repo.FinalizarDataImportProAsync(dataImportId, total, sucesso, erros.Count);

    return new ImportacaoResultado { Success = sucesso, Errors = erros };
}
```

---

## Stored Procedures

### SP 1 — `sp_DataImportPro_Create`

Cria o registro de controle e retorna o `DataImportProId` gerado.

```sql
CREATE OR ALTER PROCEDURE [dbo].[sp_DataImportPro_Create]
  @FileName  VARCHAR(200),
  @FileType  VARCHAR(20) = 'AMBOS',
  @UserId    INT = NULL
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO [dbo].[DataImportPro] (FileName, FileType, Status, DhCreate)
  VALUES (@FileName, @FileType, 'PROCESSING', GETDATE());

  SELECT SCOPE_IDENTITY() AS DataImportProId;
END
GO
```

---

### SP 2 — `sp_DataImportProProfissional_Insert`

Insere uma linha da aba Profissionais na tabela de staging.

```sql
CREATE OR ALTER PROCEDURE [dbo].[sp_DataImportProProfissional_Insert]
  @DataImportProId    INT,
  @ID              VARCHAR(50)  = NULL,
  @CodProfissional VARCHAR(50)  = NULL,
  @Email           VARCHAR(200) = NULL,
  @Nome            VARCHAR(200) = NULL,
  @Celular         VARCHAR(20)  = NULL,
  @Whats           VARCHAR(20)  = NULL,
  @CodEquipe       VARCHAR(50)  = NULL,
  @Vendedor        BIT          = NULL,
  @CodSuperior     VARCHAR(50)  = NULL
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO [dbo].[DataImportProProfissional]
    (DataImportProId, ID, CodProfissional, Email, Nome,
     Celular, Whats, CodEquipe, Vendedor, CodSuperior,
     Status, DhCreate)
  VALUES
    (@DataImportProId, @ID, @CodProfissional, @Email, @Nome,
     @Celular, @Whats, @CodEquipe, @Vendedor, @CodSuperior,
     'PENDING', GETDATE());
END
GO
```

---

### SP 3 — `sp_DataImportProCliente_Insert`

Insere uma linha da aba Clientes na tabela de staging.

```sql
CREATE OR ALTER PROCEDURE [dbo].[sp_DataImportProCliente_Insert]
  @DataImportProId INT,
  @ID           VARCHAR(50)  = NULL,
  @CodCliente   VARCHAR(50)  = NULL,
  @NomeFantasia VARCHAR(200) = NULL,
  @CNPJ         VARCHAR(20)  = NULL,
  @CodVendedor  VARCHAR(50)  = NULL
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO [dbo].[DataImportProCliente]
    (DataImportProId, ID, CodCliente, NomeFantasia, CNPJ, CodVendedor, Status, DhCreate)
  VALUES
    (@DataImportProId, @ID, @CodCliente, @NomeFantasia, @CNPJ, @CodVendedor, 'PENDING', GETDATE());
END
GO
```

---

### SP 4 — `sp_DataImportPro_Finalize`

Atualiza os totais e marca a importação como concluída.

```sql
CREATE OR ALTER PROCEDURE [dbo].[sp_DataImportPro_Finalize]
  @DataImportProId  INT,
  @TotalRows     INT,
  @ProcessedRows INT,
  @ErrorRows     INT
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE [dbo].[DataImportPro] SET
    TotalRows     = @TotalRows,
    ProcessedRows = @ProcessedRows,
    ErrorRows     = @ErrorRows,
    Status        = 'COMPLETED',
    DhUpdate      = GETDATE()
  WHERE DataImportProId = @DataImportProId;
END
GO
```

---

## Fluxo resumido

```
Frontend envia .xlsx
  └─► POST /DataImportPro/ImportarPlanilha
        └─► sp_DataImportPro_Create           → gera DataImportProId
        └─► sp_DataImportProProfissional_Insert (uma chamada por linha)
        └─► sp_DataImportProCliente_Insert     (uma chamada por linha)
        └─► sp_DataImportPro_Finalize          → marca COMPLETED
        └─► Retorna { success, errors }
```

Os registros ficam com `Status = 'PENDING'` nas tabelas de staging aguardando processamento posterior.

---

## Dependências NuGet sugeridas

- **EPPlus** — leitura de `.xlsx` sem Excel instalado
- **ClosedXML** — alternativa mais simples para leitura

```csharp
// Exemplo leitura com EPPlus
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
using var package = new ExcelPackage(stream);
var ws = package.Workbook.Worksheets["Profissionais"];
int rows = ws.Dimension?.Rows ?? 0;
for (int i = 2; i <= rows; i++) // linha 1 = cabeçalho
{
    var nome = ws.Cells[i, 4].Text; // coluna D = Nome
    // ...
}
```
