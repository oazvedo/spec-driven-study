---
name: angular-crud
description: Receita para criar ou alterar uma feature CRUD no frontend Angular deste repositório (Feature-Sliced Design com app, pages, widgets, features, entities e shared, NG-ZORRO, signals, reactive forms tipados, Vitest e Playwright). Use sempre que for criar ou alterar telas, componentes, services HTTP, rotas ou testes no frontend.
---

# Skill: angular-crud

Regras completas no ADR 0009 (FSD + NG-ZORRO), no ADR 0008 (contrato REST) e no ADR 0010 (testes).
Os exemplos usam `usuario`. Para outra entidade, troque o nome.

> A base comum (`shared/api` com `PagedResult<T>`, interceptor de ProblemDetails e helper de erros de
> formulário; o layout em `app/`; o proxy `/api`) é criada pela spec 001. Aqui só se adiciona uma entidade.

## Regra de ouro do FSD
Os imports só vão **para baixo**: `app → pages → widgets → features → entities → shared`.
Uma slice não importa outra slice da mesma camada. Toda slice exporta só pelo seu `index.ts`.
`npm run lint` falha se a regra for violada.

## Ordem dos passos

### 1. `entities/usuario/`: modelo + API
```
entities/usuario/
├── model/usuario.model.ts
├── api/usuario.api.ts
├── ui/usuario-status-tag.component.ts   # nz-tag Ativo/Inativo (opcional)
└── index.ts
```
```ts
// model/usuario.model.ts: espelha os DTOs do backend (ADR 0007)
export interface Usuario { id: string; nome: string; email: string; ativo: boolean; createdAt: string; }
export interface CriarUsuarioRequest { nome: string; email: string; }
export type AtualizarUsuarioRequest = CriarUsuarioRequest;
```
```ts
// api/usuario.api.ts: um método por endpoint do ADR 0008
@Injectable({ providedIn: 'root' })
export class UsuarioApi {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/usuarios';

  listar(page: number, pageSize: number) {
    return this.http.get<PagedResult<Usuario>>(this.url, { params: { page, pageSize } });
  }
  obter(id: string) { return this.http.get<Usuario>(`${this.url}/${id}`); }
  criar(request: CriarUsuarioRequest) { return this.http.post<Usuario>(this.url, request); }
  atualizar(id: string, request: AtualizarUsuarioRequest) { return this.http.put<Usuario>(`${this.url}/${id}`, request); }
  ativar(id: string) { return this.http.patch<void>(`${this.url}/${id}/ativar`, null); }
  inativar(id: string) { return this.http.patch<void>(`${this.url}/${id}/inativar`, null); }
}
```

### 2. `features/`: ações do usuário
- **`features/usuario-form/`**: um componente para criar **e** editar.
  - Recebe `usuario = input<Usuario | null>()` e emite `salvo = output<Usuario>()`.
  - Usa um `FormGroup` tipado com `nonNullable`, e as validações espelham o validator do backend
    (obrigatório, e-mail, tamanho máximo).
  - Usa `nz-form` + `nz-form-control [nzErrorTip]`, com mensagens em português.
  - Erro 400 do servidor: aplique os `errors` do ProblemDetails nos controles (helper de `shared/api`).
    Erro 409: mensagem no campo e-mail.
  - O botão salvar fica `nzLoading` enquanto a requisição está em andamento, e o submit é bloqueado se o form for inválido.
- **`features/usuario-toggle-ativo/`**: um `nz-switch` ou botão que chama `ativar` ou `inativar`,
  com confirmação via `nz-popconfirm` ao inativar. Emite `alterado = output<void>()`.

### 3. `widgets/usuarios-table/`: listagem paginada
- `nz-table` com **paginação no servidor**: `[nzFrontPagination]="false"`, `[nzTotal]`, `[nzPageIndex]`,
  `[nzPageSize]` e `(nzQueryParams)` para trocar de página.
- Estado com signals: `page` e `pageSize` como `signal`, e os dados com
  `rxResource({ params: () => ({ page: page(), pageSize: pageSize() }), stream: ({ params }) => api.listar(params.page, params.pageSize) })`.
- `[nzLoading]` ligado ao status do resource, e um método `recarregar()` público.
- Colunas: nome, e-mail, status (`usuario-status-tag`) e ações (editar + `usuario-toggle-ativo`).
- Emite `editar = output<Usuario>()`. O widget não abre o modal; quem decide é a página.

### 4. `pages/usuarios-page/` + rota lazy
- Compõe `usuarios-table` + `usuario-form` dentro de um `nz-modal` (criar ou editar).
- Depois de `salvo`: fecha o modal, mostra `nz-message` de sucesso e chama `recarregar()` da tabela.
- Rota em `app/app.routes.ts`:
  ```ts
  { path: 'usuarios', loadComponent: () => import('../pages/usuarios-page').then(m => m.UsuariosPage) }
  ```
- Adicione o item no array `menu` do shell (`app/app.ts`), que renderiza o `nz-menu` lateral.

### 5. Testes: **sucesso e falha** (ADR 0010)
| Onde | Mínimo |
|---|---|
| `entities/usuario/api/*.spec.ts` (Vitest + `HttpTestingController`) | Cada método chama a URL e o verbo certos. |
| `features/usuario-form/*.spec.ts` | **Sucesso:** form válido chama `criar` e emite `salvo`. **Falha:** form vazio não chama a API e mostra erros; um 400 do servidor exibe o erro no campo. |
| `widgets/usuarios-table/*.spec.ts` | Trocar de página chama `listar` com `page` e `pageSize` novos. |
| `e2e/usuarios.spec.ts` (Playwright) | **Sucesso:** cadastrar usuário e vê-lo na tabela. **Falha:** submeter vazio mostra erros e não cria registro. Opcional: inativar e ver a tag mudar. |

## CSS do NG-ZORRO: só o que se usa
Os estilos do NG-ZORRO ficam em `frontend/src/ng-zorro.less`, que importa o `entry.less` base e o `entry.less`
**de cada componente usado**. O `ng-zorro-antd.min.css` completo (cerca de 570 kB) estouraria o orçamento de bundle.
O `entry.less` já traz as dependências do componente: por exemplo, `table` inclui pagination, spin e empty.
Ao usar um componente pela primeira vez (`table`, `form`, `modal`, `message`, `tag`, `popconfirm`, `button`,
`input`, `switch`...), acrescente uma linha no mesmo formato:
`@import '../node_modules/ng-zorro-antd/<componente>/style/entry.less';`
Componente sem estilo na tela = import esquecido.

## Regras gerais
- Componentes standalone, `ChangeDetectionStrategy.OnPush`, `inject()` em vez de construtor, `input()` e `output()`.
- Nada de `any`. Os tipos vêm de `entities/*/model`.
- Só NG-ZORRO para UI. Textos de interface em português (veja a skill `better-writing`) e acessibilidade
  básica: labels, foco e botões com texto (veja a skill `better-accessibility`).

## Checklist de pronto
- [ ] `entities/<x>` com model, api e `index.ts`
- [ ] `features/<x>-form` (criar/editar) e `features/<x>-toggle-ativo`
- [ ] `widgets/<x>s-table` com paginação no servidor
- [ ] `pages/<x>s-page` + rota lazy + item de menu
- [ ] `npm run lint` verde (regras do FSD)
- [ ] `npm test` verde, com sucesso e falha
- [ ] `npm run e2e` verde, com sucesso e falha
