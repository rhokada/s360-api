# Prompt — Geração de Componentes Angular para o módulo ADM CRUD

## Contexto do projeto

- Framework: Angular (versão atual do projeto)
- Estilo: Tailwind CSS
- Autenticação: JWT já implementado — o token é enviado automaticamente via interceptor HTTP no header `Authorization: Bearer <token>`
- Base URL da API: configurada em `environment.apiUrl`
- Todos os endpoints requerem autenticação

---

## Endpoints disponíveis

Todos seguem o padrão:
- `GET    /Adm{Entidade}/Select?param1=valor&param2=valor` — lista com filtros opcionais via query string
- `POST   /Adm{Entidade}/Create` — cria registro (body JSON)
- `PUT    /Adm{Entidade}/Update` — atualiza registro (body JSON, inclui ID)
- `DELETE /Adm{Entidade}/Delete/{id}` — remove registro

Entidades: `AdmUser`, `AdmHierarchy`, `AdmHierarchyTeam`, `AdmAddress`, `AdmCompany`, `AdmCompanyDept`, `AdmDeptUser`

---

## Estrutura de dados (responses da API em camelCase)

### AdmUser
```typescript
interface AdmUser {
  userId: number;
  appId: string | null;
  name: string;
  email: string;
  dddCell: string | null;
  nrCell: string | null;
  active: boolean | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  contractId: number | null;
  pbiLogin: string | null;
  title: string | null;          // via DeptUser (OUTER APPLY)
  companyCodeUser: string | null; // via DeptUser (OUTER APPLY)
}
```

### AdmHierarchy
```typescript
interface AdmHierarchy {
  hierarchyId: number;
  name: string;
  dhCreate: string | null;
  dhUpdate: string | null;
}
```

### AdmHierarchyTeam
```typescript
interface AdmHierarchyTeam {
  hierarchyTeamId: number;
  hierarchyId: number;
  hierarchyName: string;
  userId: number;
  userName: string;
  userEmail: string;
  bossId: number;
  bossName: string;
  bossEmail: string;
  active: boolean | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  deptUserId: number | null;
  companyDeptId: number | null;
  title: string | null;
  companyCodeUser: string | null;
  deptName: string | null;
  companyId: number | null;
  companyName: string | null;
}
```

### AdmAddress
```typescript
interface AdmAddress {
  addressId: number;
  street: string | null;
  street2: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  country: string | null;
  dhUpdate: string | null;
}
```

### AdmCompany
```typescript
interface AdmCompany {
  companyId: number;
  addressId: number | null;
  groupCompanyId: number | null;
  name: string;
  taxID: string | null;
  logoUrl: string | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  parentCompanyId: number | null;
  city: string | null;   // via Address JOIN
  state: string | null;  // via Address JOIN
}
```

### AdmCompanyDept
```typescript
interface AdmCompanyDept {
  companyDeptId: number;
  companyId: number;
  companyName: string;
  addressId: number | null;
  name: string;
  profitCenter: string | null;
  costCenter: string | null;
  dhUpdate: string | null;
  city: string | null;   // via Address JOIN
  state: string | null;  // via Address JOIN
}
```

### AdmDeptUser
```typescript
interface AdmDeptUser {
  deptUserId: number;
  userId: number;
  userName: string;
  userEmail: string;
  companyDeptId: number;
  deptName: string;
  companyName: string;
  title: string | null;
  companyCodeUser: string | null;
  dhUpdate: string | null;
}
```

---

## Telas a gerar

---

### 1. Tela: Empresa (adm-company)

**Rota sugerida:** `/adm/company`

**Comportamento:**
- Exibe lista de empresas em tabela com colunas: Nome, CNPJ (taxID), Cidade/Estado (do endereço), Empresa pai
- Filtros no topo: campo de texto para Nome (debounce 400ms), campo TaxID
- Botão "Nova Empresa" abre painel lateral (side panel / drawer) com formulário
- Edição inline: clicar na linha abre o mesmo painel lateral preenchido
- Cada linha tem botão "Departamentos" que navega para `/adm/company/:companyId/dept` ou abre painel de departamentos no mesmo contexto
- Botão deletar com confirmação

**Endpoints:**
- `GET /AdmCompany/Select` — parâmetros: `name`, `taxID`, `groupCompanyId`, `parentCompanyId`
- `POST /AdmCompany/Create` — body: `{ addressId, groupCompanyId, name, taxID, logoUrl, parentCompanyId }`
- `PUT /AdmCompany/Update` — body: `{ companyId, addressId, groupCompanyId, name, taxID, logoUrl, parentCompanyId }`
- `DELETE /AdmCompany/Delete/{companyId}`

**Componente:**
```
adm-company/
  adm-company.component.ts
  adm-company.component.html
  adm-company.component.scss
```

---

### 2. Tela: Departamento (adm-company-dept)

**Rota sugerida:** `/adm/company/:companyId/dept` ou como sub-rota dentro de company

**Comportamento:**
- Exibe lista de departamentos da empresa selecionada
- Breadcrumb mostrando: "Empresas > {CompanyName} > Departamentos"
- Filtro por Nome
- Edição inline via painel lateral
- Cada linha tem botão "Funcionários" que abre DeptUser filtrado por companyDeptId
- Botão deletar com confirmação

**Endpoints:**
- `GET /AdmCompanyDept/Select` — parâmetros: `companyId` (obrigatório no contexto), `name`
- `POST /AdmCompanyDept/Create` — body: `{ companyId, addressId, name, profitCenter, costCenter }`
- `PUT /AdmCompanyDept/Update` — body: `{ companyDeptId, companyId, addressId, name, profitCenter, costCenter }`
- `DELETE /AdmCompanyDept/Delete/{companyDeptId}`

**Componente:**
```
adm-company-dept/
  adm-company-dept.component.ts
  adm-company-dept.component.html
  adm-company-dept.component.scss
```

---

### 3. Tela: Funcionários do Departamento (adm-dept-user)

**Rota sugerida:** `/adm/company/:companyId/dept/:companyDeptId/users` ou como sub-rota

**Comportamento:**
- Exibe lista de vínculos usuário-departamento
- Breadcrumb: "Empresas > {CompanyName} > {DeptName} > Funcionários"
- Colunas: Nome do usuário, E-mail, Cargo (title), Código (companyCodeUser)
- Edição inline via painel lateral
- Seleção de usuário via autocomplete (chamar `GET /AdmUser/Select?name=...`)
- Botão deletar com confirmação

**Endpoints:**
- `GET /AdmDeptUser/Select` — parâmetros: `companyDeptId` (obrigatório no contexto), `userId`, `title`, `companyCodeUser`
- `POST /AdmDeptUser/Create` — body: `{ userId, companyDeptId, title, companyCodeUser }`
- `PUT /AdmDeptUser/Update` — body: `{ deptUserId, userId, companyDeptId, title, companyCodeUser }`
- `DELETE /AdmDeptUser/Delete/{deptUserId}`

**Componente:**
```
adm-dept-user/
  adm-dept-user.component.ts
  adm-dept-user.component.html
  adm-dept-user.component.scss
```

---

### 4. Tela: Usuários (adm-user)

**Rota sugerida:** `/adm/user`

**Comportamento:**
- Tela independente de manutenção de usuários
- NUNCA exibir campos: senha, token, psw
- Colunas: Nome, E-mail, Ativo, DDD+Celular, Cargo (title do DeptUser principal), Código (companyCodeUser)
- Filtros: Nome (LIKE), E-mail (LIKE), Ativo (select: Todos/Ativo/Inativo)
- Edição via painel lateral
- Deletar realiza soft delete (active = false via SP)

**Endpoints:**
- `GET /AdmUser/Select` — parâmetros: `userId`, `name`, `email`, `active`, `contractId`
- `POST /AdmUser/Create` — body: `{ appId, name, email, dddCell, nrCell, active, contractId, pbiLogin }`
- `PUT /AdmUser/Update` — body: `{ userId, appId, name, email, dddCell, nrCell, active, contractId, pbiLogin }`
- `DELETE /AdmUser/Delete/{userId}`

**Componente:**
```
adm-user/
  adm-user.component.ts
  adm-user.component.html
  adm-user.component.scss
```

---

### 5. Tela: Hierarquia (adm-hierarchy)

**Rota sugerida:** `/adm/hierarchy`

**Comportamento:**
- Lista simples de hierarquias
- Colunas: Nome, Data de criação
- Edição inline (campo nome editável direto na linha) ou painel lateral simples
- Botão deletar com confirmação

**Endpoints:**
- `GET /AdmHierarchy/Select` — parâmetros: `hierarchyId`, `name`
- `POST /AdmHierarchy/Create` — body: `{ name }`
- `PUT /AdmHierarchy/Update` — body: `{ hierarchyId, name }`
- `DELETE /AdmHierarchy/Delete/{hierarchyId}`

**Componente:**
```
adm-hierarchy/
  adm-hierarchy.component.ts
  adm-hierarchy.component.html
  adm-hierarchy.component.scss
```

---

### 6. Tela: Equipe Hierárquica — TreeView (adm-hierarchy-team)

**Rota sugerida:** `/adm/hierarchy-team`

**Comportamento:**
- Seletor de Hierarquia no topo (dropdown com dados de `GET /AdmHierarchy/Select`)
- Ao selecionar uma hierarquia, carrega `GET /AdmHierarchyTeam/Select?hierarchyId=X`
- Exibe os dados como **árvore (TreeView)** agrupada por chefe (bossId)
- Filtro de ativo (Todos / Apenas ativos)

**Como construir a árvore a partir dos dados planos:**

A API retorna uma lista plana de registros `AdmHierarchyTeam`. Cada registro possui `userId`, `userName` e `bossId`, `bossName`. A lógica de construção da árvore é:

```typescript
interface TreeNode {
  userId: number;
  name: string;
  email: string;
  title: string | null;
  companyName: string | null;
  deptName: string | null;
  isRoot: boolean;
  members: AdmHierarchyTeam[]; // registros onde bossId === userId
  children: TreeNode[];         // sub-chefes
}

function buildTree(data: AdmHierarchyTeam[]): TreeNode[] {
  // 1. Coletar todos os bossIds únicos
  const bossIds = new Set(data.map(item => item.bossId));
  // 2. Coletar todos os userIds que também aparecem como boss
  const userIds = new Set(data.map(item => item.userId));
  // 3. Raízes: bossIds que NÃO aparecem como userId em nenhum registro
  //    (ou seja, o chefe desse nível não é subordinado de ninguém na lista)
  const rootBossIds = [...bossIds].filter(id => !userIds.has(id));
  // 4. Para cada boss raiz, criar nó recursivamente
  function makeNode(bossId: number, visited = new Set<number>()): TreeNode {
    if (visited.has(bossId)) return null; // evitar ciclos
    visited.add(bossId);
    const directReports = data.filter(item => item.bossId === bossId);
    const bossRecord = data.find(item => item.bossId === bossId);
    // Sub-bosses: userIds dos subordinados que também são boss de outros
    const subBossIds = [...new Set(directReports.map(r => r.userId))]
      .filter(uid => bossIds.has(uid));
    return {
      userId: bossId,
      name: bossRecord?.bossName ?? '',
      email: bossRecord?.bossEmail ?? '',
      title: null,
      companyName: bossRecord?.companyName ?? null,
      deptName: bossRecord?.deptName ?? null,
      isRoot: rootBossIds.includes(bossId),
      members: directReports.filter(r => !bossIds.has(r.userId)),
      children: subBossIds.map(subId => makeNode(subId, new Set(visited))).filter(Boolean)
    };
  }
  return rootBossIds.map(rootId => makeNode(rootId));
}
```

**Renderização da árvore:**
- Cada nó exibe: avatar/iniciais, nome do chefe, cargo, empresa/departamento
- Subordinados diretos (members) exibidos como chips ou linhas indentadas abaixo do chefe
- Sub-chefes exibidos como nós filhos com o mesmo padrão (recursivo com componente `adm-tree-node`)
- Expandir/colapsar nós com animação Tailwind (`transition-all`)

**Ações por nó:**
- Botão "+ Adicionar subordinado" — abre painel lateral com formulário onde bossId já vem preenchido
- Editar membro existente — clique no membro abre painel lateral com todos os campos
- Remover membro — `DELETE /AdmHierarchyTeam/Delete/{hierarchyTeamId}` com confirmação

**Endpoints:**
- `GET /AdmHierarchyTeam/Select` — parâmetros: `hierarchyId`, `bossId`, `userId`, `active`
- `POST /AdmHierarchyTeam/Create` — body: `{ hierarchyId, userId, bossId, active }`
- `PUT /AdmHierarchyTeam/Update` — body: `{ hierarchyTeamId, hierarchyId, userId, bossId, active }`
- `DELETE /AdmHierarchyTeam/Delete/{hierarchyTeamId}`

**Componentes:**
```
adm-hierarchy-team/
  adm-hierarchy-team.component.ts    (tela principal, carrega dados e monta árvore)
  adm-hierarchy-team.component.html
  adm-hierarchy-team.component.scss
  adm-tree-node/
    adm-tree-node.component.ts       (@Input() node: TreeNode — componente recursivo)
    adm-tree-node.component.html
    adm-tree-node.component.scss
```

---

## Componente compartilhado: side-panel (drawer)

Criar um componente `shared/side-panel` que:
- Recebe `@Input() title: string`
- Recebe `@Input() visible: boolean`
- Emite `@Output() closed = new EventEmitter()`
- Renderiza um drawer da direita com overlay escuro, animação slide-in/out via Tailwind
- O conteúdo é projetado via `<ng-content>`

---

## Service compartilhado por entidade

Criar um service para cada entidade seguindo o padrão:

```typescript
@Injectable({ providedIn: 'root' })
export class AdmCompanyService {
  private readonly baseUrl = `${environment.apiUrl}/AdmCompany`;

  constructor(private http: HttpClient) {}

  select(filtro: Partial<AdmCompanyFilter> = {}): Observable<AdmCompany[]> {
    return this.http.get<AdmCompany[]>(`${this.baseUrl}/Select`, { params: filtro as any });
  }

  create(model: AdmCompanyCreate): Observable<AdmCompany[]> {
    return this.http.post<AdmCompany[]>(`${this.baseUrl}/Create`, model);
  }

  update(model: AdmCompanyUpdate): Observable<AdmCompany[]> {
    return this.http.put<AdmCompany[]>(`${this.baseUrl}/Update`, model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Delete/${id}`);
  }
}
```

O interceptor de autenticação já adiciona o header `Authorization: Bearer <token>` automaticamente — não é necessário adicionar o token manualmente nos services.

---

## Observações gerais

- Usar `ReactiveFormsModule` com `FormBuilder` nos formulários
- Validações: campos `name` e `email` obrigatórios em User; `name` obrigatório em Company/Dept/Hierarchy
- Feedback visual: toast de sucesso/erro após cada operação (usar serviço de notificação já existente no projeto ou criar um simples)
- Tabelas devem ter estado de loading (skeleton ou spinner)
- Formulários devem desabilitar o botão de salvar enquanto o request está em andamento
- Confirmar deleção com modal simples ("Tem certeza? Esta ação não pode ser desfeita")
- Datas (`dhCreate`, `dhUpdate`) exibir com pipe `date:'dd/MM/yyyy HH:mm'`
- O campo `active` em User exibir como badge colorido (verde = ativo, cinza = inativo)
