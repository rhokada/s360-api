# Prompt para Agente Angular — CRUD de Surveys e Questions

## Contexto do Projeto

Você está trabalhando em um projeto Angular. A autenticação já está implementada com JWT: o token é armazenado no `localStorage` e um `HttpInterceptor` o injeta automaticamente no header `Authorization: Bearer <token>` de todas as requisições. Não é necessário implementar login ou guarda de rotas — assuma que o usuário já está autenticado.

A base URL da API é configurável via `environment.ts` (ex.: `environment.apiUrl`). Todos os endpoints exigem autenticação JWT.

O projeto usa componentes visuais no estilo shadcn (cards, tables, inputs, buttons, dialogs/modals). Use classes Tailwind CSS para estilização e prefira componentes standalone do Angular 18+.

---

## API Disponível

Todos os endpoints seguem o padrão REST abaixo. Substitua `[Entidade]` pelo nome do controller.

| Ação   | Método | Rota                          | Body / Query                |
|--------|--------|-------------------------------|-----------------------------|
| Listar | GET    | `/[Entidade]/Select`          | query params (filtros)      |
| Criar  | POST   | `/[Entidade]/Create`          | JSON body                   |
| Editar | PUT    | `/[Entidade]/Update`          | JSON body (inclui PK)       |
| Deletar| DELETE | `/[Entidade]/Delete/{id}`     | id na rota                  |

### Entidades e seus campos

#### SurveyType (`/SurveyType/...`)
- **Select (filtros opcionais):** `surveyTypeId?: number`, `surveyTypeCd?: string`, `name?: string`
- **Create:** `surveyTypeCd: string`, `name: string`
- **Update:** `surveyTypeId: number`, `surveyTypeCd: string`, `name: string`
- **Delete:** `id: number`

#### Survey (`/Survey/...`)
- **Select (filtros opcionais):** `surveyId?: number`, `surveyTypeId?: number`, `name?: string`
- **Create:** `surveyTypeId: number`, `name: string`, `dtIni: string (ISO)`, `dtFin?: string (ISO)`
- **Update:** `surveyId: number`, `surveyTypeId: number`, `name: string`, `dtIni: string`, `dtFin?: string`
- **Delete:** `id: number`

#### SurveyQuestion (`/SurveyQuestion/...`)
- **Select (filtros opcionais):** `surveyQuestionId?: number`, `surveyId?: number`, `questionId?: number`
- **Create:** `surveyId: number`, `questionId: number`
- **Update:** `surveyQuestionId: number`, `surveyId: number`, `questionId: number`
- **Delete:** `id: number`

#### SurveySup (`/SurveySup/...`)
- **Select (filtros opcionais):** `surveySupId?: number`, `supUserId?: number`, `surveyId?: number`, `name?: string`
- **Create:** `supUserId: number`, `surveyId: number`, `name: string`
- **Update:** `surveySupId: number`, `supUserId: number`, `surveyId: number`, `name: string`
- **Delete:** `id: number`

#### Question (`/Question/...`)
- **Select (filtros opcionais):** `questionId?: number`, `isComplement?: boolean`, `rank?: number`, `question?: string`, `answerTypeCd?: string`, `group?: string`, `metric?: string`, `isFirstSurvey?: boolean`, `isFinalSurvey?: boolean`, `isCompetenceLevel?: boolean`, `isFinishEarly?: boolean`, `isStandardMetric?: boolean`, `isSglYesNoType?: boolean`, `isFeedback?: boolean`
- **Create:** `isComplement: boolean`, `rank: number`, `question: string`, `description?: string`, `answerTypeCd: string`, `group?: string`, `metric?: string`, `iconMetric?: string`, `isFirstSurvey?: boolean`, `isFinalSurvey?: boolean`, `isCompetenceLevel?: boolean`, `isFinishEarly?: boolean`, `isStandardMetric?: boolean`, `isSglYesNoType?: boolean`, `isFeedback?: boolean`
- **Update:** igual ao Create + `questionId: number`
- **Delete:** `id: number`

#### QuestionOption (`/QuestionOption/...`)
- **Select (filtros opcionais):** `questionOptionId?: number`, `questionId?: number`, `complementQuestionId?: number`, `rank?: number`, `optionCd?: string`, `description?: string`, `openMsgBox?: boolean`, `needNotes?: boolean`
- **Create:** `questionId: number`, `complementQuestionId?: number`, `rank: number`, `optionCd: string`, `description?: string`, `openMsgBox?: boolean`, `needNotes?: boolean`
- **Update:** igual ao Create + `questionOptionId: number`
- **Delete:** `id: number`

---

## Telas a Implementar

### Sequência lógica de navegação

```
SurveyType (tabela mestre)
  └── Survey (filtrado por SurveyType)
        ├── SurveyQuestion (perguntas vinculadas ao survey)
        │     └── Question (banco de perguntas)
        │           └── QuestionOption (opções da pergunta)
        └── SurveySup (supervisores vinculados ao survey)
```

---

## Tela 1 — SurveyType

### Objetivo
Tela de cadastro de tipos de survey. É a tabela mestre que alimenta o campo `surveyTypeId` em Survey.

### Arquivo: `src/app/pages/survey-type/`
- `survey-type.component.ts`
- `survey-type.component.html`
- `survey-type.component.scss`

### Comportamento
- Ao carregar, busca todos os registros via `GET /SurveyType/Select` (sem filtros).
- Exibe uma tabela com colunas: **ID**, **Código (SurveyTypeCd)**, **Nome**, **Ações**.
- Linha da tabela tem botão "Editar" que transforma a linha em inputs inline (edição in-place).
- Botão "Salvar" na linha chama `PUT /SurveyType/Update`.
- Botão "Cancelar" restaura a linha.
- Botão "Excluir" chama `DELETE /SurveyType/Delete/{id}` com confirmação.
- Formulário acima da tabela (ou em modal) para criação: campos `SurveyTypeCd` e `Name`, botão "Adicionar" chama `POST /SurveyType/Create` e recarrega a lista.

### Inputs/Outputs do componente
Nenhum (tela de nível raiz).

---

## Tela 2 — Survey

### Objetivo
Lista de surveys com filtro por tipo. Acesso às telas filhas: SurveyQuestion e SurveySup.

### Arquivo: `src/app/pages/survey/`
- `survey.component.ts`
- `survey.component.html`
- `survey.component.scss`

### Comportamento
- Ao carregar, busca os tipos via `GET /SurveyType/Select` para popular um `<select>` de filtro.
- Ao selecionar um tipo (ou sem filtro), busca via `GET /Survey/Select?surveyTypeId=X`.
- Tabela com colunas: **ID**, **Tipo**, **Nome**, **Data Início**, **Data Fim**, **Ações**.
- Edição inline das colunas: `Name`, `DtIni`, `DtFin`, `SurveyTypeId` (dropdown).
- Botão "Salvar" chama `PUT /Survey/Update`.
- Botão "Excluir" chama `DELETE /Survey/Delete/{id}`.
- Formulário de criação com campos: `SurveyTypeId` (dropdown), `Name`, `DtIni` (datepicker), `DtFin` (datepicker opcional).
- Cada linha tem botão "Perguntas" que abre o componente `SurveyQuestionComponent` passando `surveyId`.
- Cada linha tem botão "Supervisores" que abre o componente `SurveySupComponent` passando `surveyId`.

### Inputs/Outputs do componente
Nenhum (tela de nível raiz). Os filhos são abertos via painel lateral ou modal.

---

## Tela 3 — SurveyQuestion (painel/modal dentro de Survey)

### Objetivo
Gerenciar quais perguntas estão vinculadas a um survey específico.

### Arquivo: `src/app/pages/survey/survey-question/`
- `survey-question.component.ts`
- `survey-question.component.html`
- `survey-question.component.scss`

### Inputs do componente
```typescript
@Input() surveyId: number;
@Output() fechar = new EventEmitter<void>();
```

### Comportamento
- Ao receber `surveyId`, busca `GET /SurveyQuestion/Select?surveyId={surveyId}`.
- Exibe tabela com colunas: **ID do Vínculo**, **ID da Pergunta**, **Texto da Pergunta** (buscar via `/Question/Select?questionId=X` ou enriquecer em etapa separada), **Ações**.
- Botão "Remover" chama `DELETE /SurveyQuestion/Delete/{id}`.
- Painel lateral ou dropdown de busca de perguntas:
  - Campo de texto para filtrar por `question` via `GET /Question/Select?question=termo`.
  - Exibe lista de resultados, ao clicar em uma pergunta chama `POST /SurveyQuestion/Create` com `{ surveyId, questionId }`.
- Botão "Fechar" emite o evento `fechar`.

---

## Tela 4 — Question

### Objetivo
Banco de perguntas. Permite criar, editar e ver as opções de cada pergunta.

### Arquivo: `src/app/pages/question/`
- `question.component.ts`
- `question.component.html`
- `question.component.scss`

### Comportamento
- Painel de filtros expansível: `question` (texto), `answerTypeCd`, `group`, `isComplement`, `isFirstSurvey`, `isFinalSurvey`, `isFeedback`.
- Botão "Buscar" chama `GET /Question/Select` com os filtros preenchidos.
- Tabela com colunas: **ID**, **Pergunta** (truncada), **Tipo de Resposta**, **Grupo**, **Rank**, **Ações**.
- Edição inline: ao clicar em "Editar" todos os campos da linha viram inputs/selects.
- Botão "Salvar" chama `PUT /Question/Update`.
- Botão "Excluir" chama `DELETE /Question/Delete/{id}`.
- Formulário de criação (accordion ou modal) com todos os campos de `QuestionCreateModel`.
- Cada linha tem botão "Opções" que abre o componente `QuestionOptionComponent` passando `questionId`.

### Inputs/Outputs do componente
Nenhum (tela de nível raiz).

---

## Tela 5 — QuestionOption (painel/modal dentro de Question)

### Objetivo
Gerenciar as opções de resposta de uma pergunta específica.

### Arquivo: `src/app/pages/question/question-option/`
- `question-option.component.ts`
- `question-option.component.html`
- `question-option.component.scss`

### Inputs do componente
```typescript
@Input() questionId: number;
@Output() fechar = new EventEmitter<void>();
```

### Comportamento
- Ao receber `questionId`, busca `GET /QuestionOption/Select?questionId={questionId}`.
- Tabela com colunas: **ID**, **Código (OptionCd)**, **Descrição**, **Rank**, **Abre Caixa de Texto** (OpenMsgBox), **Requer Notas** (NeedNotes), **Ações**.
- Edição inline por linha: ao clicar "Editar" os campos viram inputs.
- Botão "Salvar" chama `PUT /QuestionOption/Update`.
- Botão "Excluir" chama `DELETE /QuestionOption/Delete/{id}`.
- Formulário de criação no topo do painel com campos: `OptionCd`, `Rank`, `Description`, `OpenMsgBox`, `NeedNotes`, `ComplementQuestionId` (opcional).
- Botão "Fechar" emite o evento `fechar`.

---

## Tela 6 — SurveySup (painel/modal dentro de Survey)

### Objetivo
Gerenciar os supervisores vinculados a um survey.

### Arquivo: `src/app/pages/survey/survey-sup/`
- `survey-sup.component.ts`
- `survey-sup.component.html`
- `survey-sup.component.scss`

### Inputs do componente
```typescript
@Input() surveyId: number;
@Output() fechar = new EventEmitter<void>();
```

### Comportamento
- Ao receber `surveyId`, busca `GET /SurveySup/Select?surveyId={surveyId}`.
- Tabela com colunas: **ID**, **ID do Usuário (SupUserId)**, **Nome**, **Ações**.
- Edição inline: campos `SupUserId` e `Name` editáveis.
- Botão "Salvar" chama `PUT /SurveySup/Update`.
- Botão "Excluir" chama `DELETE /SurveySup/Delete/{id}`.
- Formulário de criação com campos: `SupUserId` (input numérico), `Name`.
- Botão "Fechar" emite o evento `fechar`.

---

## Estrutura de Arquivos Sugerida

```
src/app/
  pages/
    survey-type/
      survey-type.component.ts
      survey-type.component.html
      survey-type.component.scss
    survey/
      survey.component.ts
      survey.component.html
      survey.component.scss
      survey-question/
        survey-question.component.ts
        survey-question.component.html
        survey-question.component.scss
      survey-sup/
        survey-sup.component.ts
        survey-sup.component.html
        survey-sup.component.scss
    question/
      question.component.ts
      question.component.html
      question.component.scss
      question-option/
        question-option.component.ts
        question-option.component.html
        question-option.component.scss
  services/
    survey-type.service.ts
    survey.service.ts
    survey-question.service.ts
    survey-sup.service.ts
    question.service.ts
    question-option.service.ts
  models/
    survey-type.model.ts
    survey.model.ts
    survey-question.model.ts
    survey-sup.model.ts
    question.model.ts
    question-option.model.ts
```

---

## Services Angular — Padrão

Cada service deve seguir este padrão:

```typescript
// src/app/services/survey-type.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SurveyTypeService {
  private readonly base = `${environment.apiUrl}/SurveyType`;

  constructor(private http: HttpClient) {}

  select(filtro: Partial<SurveyTypeFilterModel> = {}): Observable<SurveyTypeModel[]> {
    const params = new HttpParams({ fromObject: filtro as any });
    return this.http.get<SurveyTypeModel[]>(`${this.base}/Select`, { params });
  }

  create(model: SurveyTypeCreateModel): Observable<any> {
    return this.http.post(`${this.base}/Create`, model);
  }

  update(model: SurveyTypeUpdateModel): Observable<any> {
    return this.http.put(`${this.base}/Update`, model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.base}/Delete/${id}`);
  }
}
```

Aplique o mesmo padrão para todos os outros services, apenas substituindo a entidade, os tipos e a base URL.

---

## Models TypeScript — Padrão

```typescript
// src/app/models/survey-type.model.ts
export interface SurveyTypeModel {
  surveyTypeId: number;
  surveyTypeCd: string;
  name: string;
  dhUpdate?: string;
  log?: string;
}

export interface SurveyTypeFilterModel {
  surveyTypeId?: number;
  surveyTypeCd?: string;
  name?: string;
}

export interface SurveyTypeCreateModel {
  surveyTypeCd: string;
  name: string;
}

export interface SurveyTypeUpdateModel {
  surveyTypeId: number;
  surveyTypeCd: string;
  name: string;
}
```

Crie interfaces equivalentes para todas as entidades seguindo o mesmo padrão, com os campos descritos na seção "API Disponível".

---

## Observacoes Tecnicas

- Use `HttpParams` para passar query strings no `GET /Select`, nunca coloque filtros no corpo.
- Para datas, trafegue como `string` no formato ISO 8601 (`YYYY-MM-DDTHH:mm:ss`) e formate na camada de apresentacao com `DatePipe`.
- Campos `boolean` no query string devem ser convertidos para `'true'`/`'false'` via `HttpParams`.
- Edição inline: mantenha um `Map<number, ModelUpdate>` ou clone do objeto original para controlar qual linha está em edição e reverter em caso de cancelamento.
- Ao deletar, exiba um `confirm()` ou dialog de confirmação antes de chamar a API.
- Trate erros HTTP no service ou com um interceptor global, exibindo mensagem de erro amigavel ao usuario.
- Os componentes filhos (SurveyQuestion, SurveySup, QuestionOption) devem ser exibidos como painel lateral (`aside`) ou modal, controlado por uma propriedade booleana no componente pai.
