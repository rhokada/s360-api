# PROMPT — CRUD Angular: Customer e CustomerSeller

## Contexto do projeto

- Framework: Angular (versão do projeto)
- Estilização: Tailwind CSS
- Autenticação: JWT — token enviado no header `Authorization: Bearer <token>`
- Padrão de chamada HTTP: serviços Angular com `HttpClient`, interceptor já configurado no projeto para injetar o header de autorização automaticamente
- Edição inline nas listagens (sem modal separado, exceto onde indicado)

---

## 1. Interfaces TypeScript

### Customer

```typescript
export interface Customer {
  customerId: number;
  companyId: number;
  companyName: string;
  toBeConfirmed: boolean | null;
  customerCode: string | null;
  name: string;
  cnpj: string | null;
  street: string | null;
  street2: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  log: string | null;
  guId: string | null;
  originCd: string | null;
  dataImportId: number | null;
}

export interface CustomerFilter {
  companyId?: number | null;
  name?: string | null;
  customerCode?: string | null;
  cnpj?: string | null;
  city?: string | null;
  state?: string | null;
  toBeConfirmed?: boolean | null;
}

export interface CustomerCreate {
  companyId: number;
  toBeConfirmed?: boolean | null;
  customerCode?: string | null;
  name: string;
  cnpj?: string | null;
  street?: string | null;
  street2?: string | null;
  neighborhood?: string | null;
  city?: string | null;
  state?: string | null;
  zipCode?: string | null;
  originCd?: string | null;
  dataImportId?: number | null;
}

export interface CustomerUpdate extends CustomerCreate {
  customerId: number;
}
```

### CustomerSeller

```typescript
export interface CustomerSeller {
  customerSellerId: number;
  customerId: number;
  customerName: string;
  customerCode: string | null;
  sellerUserId: number;
  sellerName: string;
  sellerEmail: string;
  active: boolean;
  dhCreate: string | null;
  dhUpdate: string | null;
  log: string | null;
}

export interface CustomerSellerFilter {
  customerId?: number | null;
  sellerUserId?: number | null;
  active?: boolean | null;
}

export interface CustomerSellerCreate {
  customerId: number;
  sellerUserId: number;
  active?: boolean | null;
}

export interface CustomerSellerUpdate extends CustomerSellerCreate {
  customerSellerId: number;
}
```

---

## 2. Endpoints da API

Base URL: conforme `environment.apiUrl` do projeto.

### Customer

| Ação   | Método | Endpoint                              | Body / Query                   |
|--------|--------|---------------------------------------|--------------------------------|
| Listar | GET    | `/AdmCustomer/Select`                 | query params: `CustomerFilter` |
| Criar  | POST   | `/AdmCustomer/Create`                 | body: `CustomerCreate`         |
| Editar | PUT    | `/AdmCustomer/Update`                 | body: `CustomerUpdate`         |
| Deletar| DELETE | `/AdmCustomer/Delete/{customerId}`    | path param                     |

### CustomerSeller

| Ação   | Método | Endpoint                                         | Body / Query                         |
|--------|--------|--------------------------------------------------|--------------------------------------|
| Listar | GET    | `/AdmCustomerSeller/Select`                      | query params: `CustomerSellerFilter` |
| Criar  | POST   | `/AdmCustomerSeller/Create`                      | body: `CustomerSellerCreate`         |
| Editar | PUT    | `/AdmCustomerSeller/Update`                      | body: `CustomerSellerUpdate`         |
| Deletar| DELETE | `/AdmCustomerSeller/Delete/{customerSellerId}`   | path param                           |

---

## 3. Sequência lógica de telas

### 3.1 Tela Customer (`/adm/customers`)

**Filtros** (barra superior, aplicados via botão "Buscar"):
- `companyId` — select ou input numérico
- `name` — input texto (busca parcial, LIKE na SP)
- `city` — input texto
- `state` — input texto
- `toBeConfirmed` — select: Todos / Sim / Não

**Listagem** (tabela com colunas):
- CustomerCode, Name, CompanyName, CNPJ, City, State, ToBeConfirmed, DhCreate

**Ações por linha**:
- Botão "Editar" — abre edição inline (ou expande a linha) nos campos editáveis: CompanyId, ToBeConfirmed, CustomerCode, Name, CNPJ, Street, Street2, Neighborhood, City, State, ZipCode, OriginCd
- Botão "Vendedores" — navega para a tela CustomerSeller pré-filtrando pelo `customerId` da linha selecionada (pode ser via rota `/adm/customers/:customerId/sellers` ou abertura de painel lateral)
- Botão "Excluir" — confirmação antes de deletar

**Botão "Novo Cliente"** (topo da tela):
- Abre formulário (inline ou modal) com os campos de `CustomerCreate`
- `companyId` obrigatório; `name` obrigatório

---

### 3.2 Tela CustomerSeller (`/adm/customers/:customerId/sellers` ou painel lateral)

Quando aberta a partir da tela Customer, o filtro `customerId` deve ser preenchido automaticamente com o id do cliente selecionado e o campo deve ser somente leitura (não editável pelo usuário nesse contexto).

**Cabeçalho da tela**: exibir `CustomerName` e `CustomerCode` do cliente pai para contextualização.

**Filtros** (adicionais, além do `customerId` fixo):
- `sellerUserId` — autocomplete de usuário (campo de busca por nome, retorna `userId` + `name` + `email`)
- `active` — select: Todos / Ativo / Inativo

**Listagem** (tabela com colunas):
- SellerName, SellerEmail, Active, DhCreate

**Ações por linha**:
- Botão "Editar" — edição inline: pode alterar `active` e `sellerUserId`
- Botão "Excluir" — confirmação antes de deletar

**Botão "Vincular Vendedor"** (topo da tela):
- Abre formulário com:
  - `sellerUserId` — autocomplete: busca usuários pelo nome (endpoint existente no projeto), retorna objeto com `userId`, `name`, `email`
  - `active` — checkbox (padrão: true)
- O `customerId` é preenchido automaticamente pelo contexto

---

## 4. Estrutura de arquivos sugerida

```
src/app/adm/
  customer/
    customer.component.ts
    customer.component.html
    customer.component.scss
    customer.service.ts
  customer-seller/
    customer-seller.component.ts
    customer-seller.component.html
    customer-seller.component.scss
    customer-seller.service.ts
  models/
    customer.model.ts       (interfaces Customer, CustomerFilter, CustomerCreate, CustomerUpdate)
    customer-seller.model.ts (interfaces CustomerSeller, CustomerSellerFilter, CustomerSellerCreate, CustomerSellerUpdate)
```

---

## 5. Serviços Angular (esboço)

### CustomerService

```typescript
@Injectable({ providedIn: 'root' })
export class CustomerService {
  private apiUrl = `${environment.apiUrl}/AdmCustomer`;

  constructor(private http: HttpClient) {}

  select(filtro: CustomerFilter): Observable<Customer[]> {
    return this.http.get<Customer[]>(`${this.apiUrl}/Select`, { params: filtro as any });
  }

  create(model: CustomerCreate): Observable<Customer[]> {
    return this.http.post<Customer[]>(`${this.apiUrl}/Create`, model);
  }

  update(model: CustomerUpdate): Observable<Customer[]> {
    return this.http.put<Customer[]>(`${this.apiUrl}/Update`, model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }
}
```

### CustomerSellerService

```typescript
@Injectable({ providedIn: 'root' })
export class CustomerSellerService {
  private apiUrl = `${environment.apiUrl}/AdmCustomerSeller`;

  constructor(private http: HttpClient) {}

  select(filtro: CustomerSellerFilter): Observable<CustomerSeller[]> {
    return this.http.get<CustomerSeller[]>(`${this.apiUrl}/Select`, { params: filtro as any });
  }

  create(model: CustomerSellerCreate): Observable<CustomerSeller[]> {
    return this.http.post<CustomerSeller[]>(`${this.apiUrl}/Create`, model);
  }

  update(model: CustomerSellerUpdate): Observable<CustomerSeller[]> {
    return this.http.put<CustomerSeller[]>(`${this.apiUrl}/Update`, model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }
}
```

---

## 6. Observações de implementação

- O header `Authorization: Bearer <token>` deve ser injetado pelo interceptor HTTP já existente no projeto. Não adicionar o header manualmente nos serviços.
- Parâmetros `null` ou `undefined` nos filtros devem ser omitidos da query string para evitar que a SP receba string `"null"` como valor.
- A resposta de Create e Update retorna a entidade persistida (com JOINs), use-a para atualizar a linha na listagem sem recarregar tudo.
- A resposta de Delete retorna `[{ customerId: X, status: "deleted" }]` — use para remover a linha da listagem local.
- Paginação: a SP não tem OFFSET/FETCH, portanto implemente paginação no lado cliente (slice do array retornado) ou solicite adição de paginação na SP caso o volume de dados exija.
- Autocomplete de usuário para `sellerUserId`: reutilize o endpoint de usuários já existente no projeto (verificar qual endpoint retorna lista de usuários com `userId`, `name`, `email`).
