# Plano de Implementação — Dashboard Indicadores

## Visão Geral

Dashboard estilo Power BI, full-screen (sem sidebar/header), carrega todos os dados da API uma única vez e aplica todos os filtros em memória. O clique em qualquer linha ou barra de gráfico filtra todos os outros componentes; Ctrl+clique permite multi-seleção na mesma lista.

---

## 1. Backend (.NET)

### 1.1 Model — `DashIndicadoresModel.cs`

Classe que mapeia cada coluna retornada pela procedure:

| Propriedade C#           | Coluna SQL                    | Tipo      |
|--------------------------|-------------------------------|-----------|
| `SubmittedAnswerDetailId`| `SubmittedAnswerDetailId`     | `int`     |
| `IdQuestionario`         | `IdQuestionario`              | `int`     |
| `TipoQuestionario`       | `Tipo Questionário`           | `string`  |
| `Data`                   | `Data`                        | `DateTime?` |
| `Supervisor`             | `Supervisor`                  | `string`  |
| `IdUsuario`              | `IdUsuario`                   | `int`     |
| `CodVendedor`            | `Cod Vendedor`                | `string`  |
| `IdVendedor`             | `IdVendedor`                  | `int`     |
| `Vendedor`               | `Vendedor`                    | `string`  |
| `CodCliente`             | `Cod Cliente`                 | `string`  |
| `Questao`                | `Questão`                     | `string`  |
| `Rank`                   | `Rank`                        | `int?`    |
| `MetricaPadrao`          | `Metrica padrão`              | `bool`    |
| `TipoSN`                 | `Tipo S/N`                    | `bool`    |
| `Resposta`               | `Resposta`                    | `string`  |
| `Sim`                    | `SIM`                         | `int`     |
| `Nao`                    | `NÃO`                         | `int`     |
| `NaoRespondido`          | `N/R`                         | `int`     |
| `Justificado`            | `JUSTIFICADO`                 | `int`     |
| `Grupo`                  | `Grupo`                       | `string`  |
| `Metrica`                | `Métrica`                     | `string`  |

### 1.2 Interface — `IDashIndicadoresService.cs`

```csharp
dynamic Select(int userId, string tokenUsuario);
```

### 1.3 Service — `DashIndicadoresService.cs`

- Recebe `userId` e `tokenUsuario`
- Chama `SP_dash_SubmittedAnswerDetailsByUser` via Dapper com `CommandType.StoredProcedure`
- Retorna `.ToList()` via `con.Query(...)`

### 1.4 Controller — `DashIndicadoresController.cs`

- `[Authorize]`, `[Route("[controller]")]`
- `GET Select` — extrai `userId` do JWT (`User.FindFirst("SubjectId")`) e chama o service
- Usa `_camelCase` / `OkDyn` padrão do projeto

### 1.5 Startup.cs

Adicionar:
```csharp
services.AddScoped<IDashIndicadoresService, DashIndicadoresService>();
```

---

## 2. Frontend (Angular)

### 2.1 Dependência — Chart.js

```bash
npm install chart.js
```

Usado diretamente via `import { Chart } from 'chart.js/auto'` — sem wrapper adicional.

### 2.2 Rota — `app.routes.ts`

Nova rota **fora** do grupo `adm`, no nível raiz:

```typescript
{
  path: 'indicadores',
  loadComponent: () =>
    import('./pages/indicadores/indicadores.component')
      .then(m => m.IndicadoresComponent),
  canActivate: [authGuard]
}
```

### 2.3 AppComponent — `app.component.ts`

Adicionar `/indicadores` à condição de tela cheia (junto com `/login`):

```typescript
this.isLoginPage = url.includes('/login')
                || url.includes('/select-role')
                || url.includes('/indicadores');
```

Isso reutiliza a lógica existente de esconder sidebar e header.

### 2.4 Service — `indicadores.service.ts`

```typescript
select(): Observable<DashRow[]>  // GET /DashIndicadores/Select
```

O `userId` é lido do token JWT armazenado (via `AuthService`) — não precisa ser passado pelo template.

### 2.5 Interfaces — `indicadores.interfaces.ts` (novo arquivo)

```typescript
export interface DashRow {
  submittedAnswerDetailId: number;
  idQuestionario: number;
  tipoQuestionario: string;
  data: string;
  supervisor: string;
  idUsuario: number;
  codVendedor: string;
  idVendedor: number;
  vendedor: string;
  codCliente: string;
  questao: string;
  rank: number | null;
  metricaPadrao: boolean;
  tipoSN: boolean;
  resposta: string | null;
  sim: number;       // 0 ou 1
  nao: number;       // 0 ou 1
  naoRespondido: number; // 0 ou 1
  justificado: number;
  grupo: string;
  metrica: string;
}

export interface FilterState {
  tiposQuestionario: string[];
  supervisores: string[];
  datas: string[];
  vendedores: number[];
  grupos: string[];
  metricas: string[];
}
```

---

## 3. Componente Principal — `IndicadoresComponent`

### 3.1 Estado

```typescript
allRows: DashRow[]          // dados brutos da API (imutável após load)
filteredRows: DashRow[]     // dados após aplicar todos os filtros ativos
filter: FilterState         // seleções ativas em cada lista
isLoading: boolean
```

### 3.2 Lógica de filtragem

- Método `applyFilters()` recalcula `filteredRows` a partir de `allRows` testando todos os critérios ativos
- Chamado toda vez que `filter` muda
- Tempo de execução linear O(n) — adequado para o volume esperado

### 3.3 Interação (cross-filter)

- `toggle(field: keyof FilterState, value)` — adiciona/remove valor do array de seleção
- Ctrl+clique no mesmo campo: adiciona ao array sem limpar seleção anterior
- Clique simples em outro valor do mesmo campo: substitui a seleção
- Clique no valor já selecionado (sem Ctrl): limpa toda a seleção daquele campo

---

## 4. Layout e Computações

### Estrutura geral

```
┌───────────────────────────────── full viewport ─────────────────────────────────┐
│  [COLUNA 1 — ~35% largura]          [COLUNA 2 — ~65% largura]                  │
│  ┌──────────────────────────┐        ┌───────────────────────────────────────┐  │
│  │ Row 1 (menor altura)     │        │ Row 1 (totalizadores — menor altura)  │  │
│  │  Tipo Qtário | Supervisor│        │  Datas | Vendedores | Qtários | %SIM  │  │
│  ├──────────────────────────┤        ├───────────────────────────────────────┤  │
│  │ Row 2                    │        │ Row 2 — Gráfico de barras por data    │  │
│  │  Data    | Vendedor      │        │                                       │  │
│  ├──────────────────────────┤        ├───────────────────────────────────────┤  │
│  │ Row 3 (maior)            │        │ Row 3                                 │  │
│  │  Grupo (%SIM/%NÃO/%N/R)  │        │  Métrica (tabela) | %SIM/%NÃO/%N/R   │  │
│  └──────────────────────────┘        └───────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────────────────┘
```

O layout ocupa `100vh` com `display: grid` ou `flexbox` nas colunas; cada card tem `overflow-y: auto` internamente. Nenhum scroll no `body`.

---

### Coluna 1 — Row 1 (menor altura, ~15vh)

#### Card: Tipo Questionário
- Agrupa `filteredRows` por `tipoQuestionario`
- Tabela com 1 coluna: nome do tipo
- Click → `toggle('tiposQuestionario', valor)`
- Linha selecionada recebe destaque visual

#### Card: Supervisor
- Agrupa por `supervisor`
- Tabela com 1 coluna: nome
- Click → `toggle('supervisores', valor)`

---

### Coluna 1 — Row 2

#### Card: Data
- Agrupa por `data` (formatar como `dd/MM/yyyy`)
- Tabela com 1 coluna: data
- Click → `toggle('datas', valor)`

#### Card: Vendedor
- Agrupa por `idVendedor` / `vendedor`
- Tabela com 2 colunas: `Cod Vend` (codVendedor) | `Vendedor`
- Click → `toggle('vendedores', idVendedor)`

---

### Coluna 1 — Row 3 (maior altura, flex-grow)

#### Card: Grupo
- Agrupa `filteredRows` por `grupo`
- Para cada grupo calcula:
  - `%SIM  = sum(sim)  / totalRows * 100`
  - `%NÃO  = sum(nao)  / totalRows * 100`
  - `%N/R  = sum(naoRespondido) / totalRows * 100`
  - `totalRows` = contagem de linhas no grupo
- Tabela com 4 colunas: `Grupo | %SIM | %NÃO | %N/R`
- Click → `toggle('grupos', valor)`

---

### Coluna 2 — Row 1 (totalizadores — menor altura, ~10vh)

| Card | Cálculo |
|------|---------|
| **Qtd de Datas** | `new Set(filteredRows.map(r => r.data)).size` |
| **Qtd Vendedores** | `new Set(filteredRows.map(r => r.idVendedor)).size` |
| **Qtd Questionários** | `new Set(filteredRows.map(r => r.idQuestionario)).size` |
| **% SIM** | `sum(sim) / filteredRows.length * 100` arredondado para 1 decimal |

Esses cards são **read-only** (não têm interação de filtro).

---

### Coluna 2 — Row 2 — Gráfico de barras

- Agrupa `filteredRows` por `data`
- Para cada data: `totalSim`, `totalNao`, `totalNR`
- Renderiza via **Chart.js** (`Bar`) em canvas
- 3 datasets: SIM (verde), NÃO (vermelho), N/R (cinza)
- Clique em barra → `toggle('datas', dataClicada)`
- Recria o gráfico (`chart.destroy()` + novo `Chart(...)`) sempre que `filteredRows` muda

---

### Coluna 2 — Row 3

#### Sub-coluna maior: Tabela Métrica
- Agrupa por `metrica`
- Mesma lógica de `%SIM`, `%NÃO`, `%N/R` por grupo de métrica
- 4 colunas: `Métrica | %SIM | %NÃO | %N/R`
- Click → `toggle('metricas', valor)`

#### Sub-coluna menor: 3 cards totalizadores
- Calculados sobre `filteredRows` inteiros (não por grupo):
  - **% SIM total** — `sum(sim) / filteredRows.length * 100`
  - **% NÃO total** — `sum(nao) / filteredRows.length * 100`
  - **% N/R total** — `sum(naoRespondido) / filteredRows.length * 100`
- Exibir com barra de progresso visual (CSS) além do número

---

## 5. Arquivos a criar/modificar

### Novos (backend)
| Arquivo | Ação |
|---------|------|
| `Models/DashIndicadoresModel.cs` | criar |
| `Services/Interfaces/IDashIndicadoresService.cs` | criar |
| `Services/DashIndicadoresService.cs` | criar |
| `Controllers/DashIndicadoresController.cs` | criar |

### Modificar (backend)
| Arquivo | Mudança |
|---------|---------|
| `Startup.cs` | `AddScoped<IDashIndicadoresService, DashIndicadoresService>()` |

### Novos (frontend)
| Arquivo | Ação |
|---------|------|
| `src/app/pages/indicadores/indicadores.component.ts` | criar |
| `src/app/pages/indicadores/indicadores.component.html` | criar |
| `src/app/pages/indicadores/indicadores.component.scss` | criar |
| `src/app/core/services/indicadores.service.ts` | criar |
| `src/app/shared/models/indicadores.interfaces.ts` | criar |

### Modificar (frontend)
| Arquivo | Mudança |
|---------|---------|
| `app.routes.ts` | nova rota `/indicadores` |
| `app.component.ts` | adicionar `/indicadores` à condição full-screen |

### Dependência npm
```bash
npm install chart.js
```

---

## 6. Pontos de Atenção

1. **UserId**: O controller extrai o `userId` do claim JWT (`SubjectId`) — o frontend não precisa enviar na query string.
2. **Datas nulas**: Linhas com `data` nula devem ser tratadas como `"Sem data"` no agrupamento.
3. **Grupos/Métricas nulos**: Mesmo tratamento — exibir como `"(sem grupo)"` ou omitir, conforme preferência.
4. **Gráfico**: Ao destruir/recriar o Chart.js é necessário guardar a instância no componente e chamar `.destroy()` antes de recriar para evitar memory leak.
5. **Responsividade**: Em mobile o layout colapsa para coluna única com scroll no body (exceção à regra desktop).
6. **Performance**: Para datasets grandes, o `applyFilters()` pode ser chamado dentro de `requestAnimationFrame` para não bloquear a UI.
