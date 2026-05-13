# Plano de Implementação — AiPrompt e TranscricaoAudio

---

## Convenções do Projeto (base para ambas as tabelas)

| Camada | Padrão |
|---|---|
| SP | `@TypeRequest VARCHAR(10)` + params opcionais; `GRANT EXECUTE TO [S360sys]` no final |
| Service | Retorna `dynamic`; chama `con.Query("SP_Nome", new { TypeRequest = "SELECT", ... }, commandType: ...)` |
| Controller | `[Authorize]`; helper `OkDyn()` com camelCase; extrai token via `Request.Headers["Authorization"]` |
| Frontend service | `HttpClient` direto; base = `${environment.apiUrl}/NomeController` |
| Frontend component | Standalone; `ReactiveFormsModule`; `SidePanelComponent` para criar/editar; `ToastService` para feedback |

---

## Tabela 1 — AiPrompt

### 1.1 Stored Procedure — `SP_Adm_AiPrompt`

**Arquivo:** `SqlScripts/Adm/SP_Adm_AiPrompt.sql`

**Parâmetros:**
```sql
@TypeRequest  VARCHAR(10)       -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
@AiPromptId   INT            = NULL
@AiProcessCd  NVARCHAR(500)  = NULL
@Context      NVARCHAR(4000) = NULL
@Prompt       VARCHAR(MAX)   = NULL
@Engine       NVARCHAR(1000) = NULL
@Log          VARCHAR(MAX)   = NULL
@token_usuario NVARCHAR(MAX) = NULL
```

**Regras por TypeRequest:**

| TypeRequest | Lógica |
|---|---|
| `SELECT` | `WHERE Active = 1` — retorna apenas registros ativos |
| `INSERT` | `UPDATE AiPrompt SET Active = 0 WHERE AiProcessCd = @AiProcessCd` → depois `INSERT` com `Active = 1` → retorna `SCOPE_IDENTITY()` |
| `UPDATE` | Lê o `AiProcessCd` do registro existente pelo `@AiPromptId` → desativa todos os registros do mesmo `AiProcessCd` → insere novo registro com `Active = 1` (versioning, não altera o registro original) → retorna novo id |
| `DELETE` | Soft delete: `UPDATE AiPrompt SET Active = 0 WHERE AiPromptId = @AiPromptId` (não há botão de delete no frontend, mas a SP inclui para consistência) |

> **Regra crítica do UPDATE:** nunca altera o registro existente. Cria uma nova linha. Isso garante histórico de versões dos prompts. O `AiProcessCd` do novo registro vem do `@AiProcessCd` passado (o usuário pode alterar o código também).

---

### 1.2 Backend .NET

**`Models/AiPromptModel.cs`**
```
AiPromptFilterModel  → AiPromptId? (filtro opcional)
AiPromptCreateModel  → AiProcessCd, Context, Prompt, Engine, Log?
AiPromptUpdateModel  → AiPromptId, AiProcessCd, Context, Prompt, Engine, Log?
AiPromptRow (read)   → AiPromptId, AiProcessCd, Context, Prompt, Engine, Dhcreate, Active, Log
```

**`Services/Interfaces/IAiPromptService.cs`**
```
Select(AiPromptFilterModel, tokenUsuario) → dynamic
Create(AiPromptCreateModel, tokenUsuario) → dynamic
Update(AiPromptUpdateModel, tokenUsuario) → dynamic
Delete(int id, tokenUsuario)              → dynamic
```

**`Services/AiPromptService.cs`**  
Cada método chama `SP_Adm_AiPrompt` com o `TypeRequest` correspondente. Segue exatamente o mesmo padrão do `AdmRoleService`.

**`Controllers/AiPromptController.cs`**  
Rotas:
```
GET    /AiPrompt/Select
POST   /AiPrompt/Create
PUT    /AiPrompt/Update
DELETE /AiPrompt/Delete/{id}
```
Sem rota de Delete no frontend, mas o endpoint existe na API.

**`Startup.cs`** — adicionar:
```csharp
services.AddScoped<IAiPromptService, AiPromptService>();
```

---

### 1.3 Frontend Angular

**`core/services/ai-prompt.service.ts`**
```typescript
select()           → GET  /AiPrompt/Select
create(body)       → POST /AiPrompt/Create
update(body)       → PUT  /AiPrompt/Update
// delete não é exposto no frontend
```

**`shared/models/ai-prompt.interfaces.ts`**
```typescript
export interface AiPromptItem {
  aiPromptId:  number;
  aiProcessCd: string;
  context:     string;
  prompt:      string;
  engine:      string;
  dhcreate:    string | null;
  active:      boolean | null;
  log:         string | null;
}
```

**`pages/adm/adm-ai-prompt/`** — componente standalone com:

- Tabela listando: `AiProcessCd`, `Engine`, `Context` (truncado), `Dhcreate`
- Filtro por `AiProcessCd` (debounce em memória)
- Botão "Novo" → abre `SidePanelComponent` com formulário de criação
- Botão de editar em cada linha (ícone de lápis) → abre `SidePanelComponent` preenchido
- **Sem** botão de deletar
- Formulário no panel: `AiProcessCd`, `Context` (textarea), `Prompt` (textarea grande), `Engine`, `Log` (textarea opcional)
- `save()`: se `isEditing` → `update()`, senão → `create()`
- Após salvar: fecha panel + reload

**`app.routes.ts`** — adicionar dentro de `adm`:
```typescript
{
  path: 'ai-prompt',
  loadComponent: () => import('./pages/adm/adm-ai-prompt/adm-ai-prompt.component').then(m => m.AdmAiPromptComponent),
  data: { slug: 'adm-ai-prompt' }
}
```

---

## Tabela 2 — TranscricaoAudio (SubmittedAnswerDetails)

### 2.1 Stored Procedure — `SP_Adm_TranscricaoAudio`

**Arquivo:** `SqlScripts/Adm/SP_Adm_TranscricaoAudio.sql`

**Parâmetros:**
```sql
@TypeRequest   VARCHAR(10)    -- apenas 'SELECT'
@token_usuario NVARCHAR(MAX) = NULL
```

**SELECT:**
```sql
SELECT
    SubmittedAnswerDetailId,
    AlternativeId,
    AnswerText,
    QuestionGroup,
    Metric,
    AudioTranscription,
    AiAnalysis
FROM SubmittedAnswerDetails
WHERE AudioTranscription IS NOT NULL
  AND AudioTranscription <> ''
ORDER BY SubmittedAnswerDetailId DESC
```

---

### 2.2 Backend .NET

**`Models/TranscricaoAudioModel.cs`**
```
TranscricaoAudioRow → SubmittedAnswerDetailId, AlternativeId, AnswerText, QuestionGroup, Metric, AudioTranscription, AiAnalysis
```
(Sem models de Create/Update/Delete — somente SELECT)

**`Services/Interfaces/ITranscricaoAudioService.cs`**
```
Select(tokenUsuario) → dynamic
```

**`Services/TranscricaoAudioService.cs`**  
Chama `SP_Adm_TranscricaoAudio` com `TypeRequest = "SELECT"`.

**`Controllers/TranscricaoAudioController.cs`**
```
GET /TranscricaoAudio/Select
```

**`Startup.cs`** — adicionar:
```csharp
services.AddScoped<ITranscricaoAudioService, TranscricaoAudioService>();
```

---

### 2.3 Frontend Angular

**`core/services/transcricao-audio.service.ts`**
```typescript
select() → GET /TranscricaoAudio/Select
```

**`shared/models/transcricao-audio.interfaces.ts`**
```typescript
export interface TranscricaoAudioItem {
  submittedAnswerDetailId: number;
  alternativeId:     string | null;
  answerText:        string | null;
  questionGroup:     string | null;
  metric:            string | null;
  audioTranscription: string | null;  // JSON string
  aiAnalysis:        string | null;   // JSON string
}

export interface TranscricaoSentence {
  text:         string;
  speaker_name: string;
  start_time:   number;
  end_time:     number;
}
```

**`pages/adm/adm-transcricao-audio/`** — componente standalone com:

#### Listagem
- Colunas: `AlternativeId`, `AnswerText`, `QuestionGroup`, `Metric`
- Filtro por texto (debounce em memória, busca em todas as colunas)
- **Sem** botão de ação nas linhas
- Clicar em qualquer parte da linha → abre o modal de detalhe

#### Modal centralizado
- CSS: posição fixa, centralizado horizontal e vertical, largura mínima de 60% da tela (desktop), 95% mobile
- Implementado diretamente no componente (sem componente separado) via `*ngIf` + classe CSS
- Botão de fechar (X) no canto superior direito
- Overlay escuro clicável fecha o modal
- Duas abas: **Transcrição** | **Análise**

#### Tab 1 — Transcrição (layout chat WhatsApp)
- Parse `audioTranscription` (JSON string) → array de `TranscricaoSentence`
- Identificar speakers únicos e atribuir: lado (esquerda/direita) e cor de fundo
  - Speaker 1 → direita, cor azul (`#DCF8C6` ou similar)
  - Speaker 2 → esquerda, cor cinza claro (`#F0F0F0`)
  - Speakers adicionais → cores alternativas
- Cada bolha de mensagem exibe: texto + horário (`start_time` formatado em `mm:ss`)
- Layout: `display: flex; flex-direction: column`; bolhas com `align-self: flex-end` (direita) ou `flex-start` (esquerda)
- Nome do speaker acima da primeira bolha de cada sequência

#### Tab 2 — Análise
- Parse `aiAnalysis` (JSON string) → objeto
- Iterar as chaves do JSON:
  - Se o valor for **string/number**: exibir `<label>chave</label>: valor` na mesma linha
  - Se o valor for **array de objetos**: exibir tabela com os atributos do primeiro objeto como cabeçalhos e as linhas como rows
  - Se o valor for **array de strings**: exibir lista `<ul>`
- Nomes de chaves formatados: `snake_case` → substituir `_` por espaço + capitalizar primeira letra

**`app.routes.ts`** — adicionar dentro de `adm`:
```typescript
{
  path: 'transcricao-audio',
  loadComponent: () => import('./pages/adm/adm-transcricao-audio/adm-transcricao-audio.component').then(m => m.AdmTranscricaoAudioComponent),
  data: { slug: 'adm-transcricao-audio' }
}
```

---

## Arquivos a criar

### Backend
| Arquivo | Tipo |
|---|---|
| `SqlScripts/Adm/SP_Adm_AiPrompt.sql` | NOVO |
| `SqlScripts/Adm/SP_Adm_TranscricaoAudio.sql` | NOVO |
| `Models/AiPromptModel.cs` | NOVO |
| `Models/TranscricaoAudioModel.cs` | NOVO |
| `Services/Interfaces/IAiPromptService.cs` | NOVO |
| `Services/Interfaces/ITranscricaoAudioService.cs` | NOVO |
| `Services/AiPromptService.cs` | NOVO |
| `Services/TranscricaoAudioService.cs` | NOVO |
| `Controllers/AiPromptController.cs` | NOVO |
| `Controllers/TranscricaoAudioController.cs` | NOVO |
| `Startup.cs` | MODIFICAR (2 linhas AddScoped) |

### Frontend
| Arquivo | Tipo |
|---|---|
| `core/services/ai-prompt.service.ts` | NOVO |
| `core/services/transcricao-audio.service.ts` | NOVO |
| `shared/models/ai-prompt.interfaces.ts` | NOVO |
| `shared/models/transcricao-audio.interfaces.ts` | NOVO |
| `pages/adm/adm-ai-prompt/adm-ai-prompt.component.ts` | NOVO |
| `pages/adm/adm-ai-prompt/adm-ai-prompt.component.html` | NOVO |
| `pages/adm/adm-ai-prompt/adm-ai-prompt.component.scss` | NOVO |
| `pages/adm/adm-transcricao-audio/adm-transcricao-audio.component.ts` | NOVO |
| `pages/adm/adm-transcricao-audio/adm-transcricao-audio.component.html` | NOVO |
| `pages/adm/adm-transcricao-audio/adm-transcricao-audio.component.scss` | NOVO |
| `app.routes.ts` | MODIFICAR (2 rotas dentro de `adm`) |
