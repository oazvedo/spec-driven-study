# 0009. Frontend Angular com Feature-Sliced Design e NG-ZORRO

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
O frontend precisa de uma organização que escale por feature, com regras claras de dependência,
e de uma biblioteca de componentes completa para telas administrativas (tabelas, formulários, modais).

## Decisão
**Angular** (versão estável atual) com standalone components, signals, reactive forms tipados e rotas lazy.

**Feature-Sliced Design** em `frontend/src/app/`:
```
app/       # bootstrap, providers, rotas raiz, layout (nz-layout)
pages/     # páginas roteáveis: compõem widgets e features (ex.: usuarios-list-page)
widgets/   # blocos de UI compostos (ex.: usuarios-table)
features/  # ações do usuário com valor de negócio (ex.: usuario-form, usuario-toggle-ativo)
entities/  # entidades de negócio: model (tipos), api (HTTP), ui pequena (ex.: usuario-status-tag)
shared/    # sem domínio: api base (PagedResult<T>, interceptor de erro), ui, utils, config
```
Regras:
- **Imports só para baixo:** `app → pages → widgets → features → entities → shared`. Uma slice não
  importa outra slice da mesma camada.
- Cada slice expõe uma API pública em `index.ts`. Ninguém importa arquivos internos de outra slice.
- Segmentos dentro de uma slice: `ui/`, `model/`, `api/`, conforme a necessidade.
- As regras são verificadas no lint (ESLint com `eslint-plugin-boundaries`).

**NG-ZORRO** é a única biblioteca de UI: `nz-table` com paginação no servidor (`nzFrontPagination=false`),
`nz-form`, `nz-modal`, `nz-message`, `nz-tag` e `nz-layout`. Nada de CSS framework adicional.

**Estado:** signals nos componentes e services. O HTTP retorna `Observable` e é convertido com
`toSignal` ou `rxResource` quando fizer sentido. Sem NgRx por enquanto.

**Erros:** um interceptor em `shared/api` lê o ProblemDetails (ADR 0006). Os erros por campo (`errors`)
são exibidos no `nz-form`, e os demais aparecem em `nz-message`.

## Alternativas consideradas
- **core/shared/features:** mais conhecida no Angular, mas não define regras de dependência entre as features.
- **Angular Material / PrimeNG:** o NG-ZORRO (Ant Design) foi escolhido por ter componentes administrativos mais completos.
- **NgRx:** é demais para um CRUD. Pode entrar em um ADR futuro.

## Consequências
- ✅ Cada pasta tem um papel claro, e o lint impede acoplamento entre slices.
- ⚠️ FSD é menos comum no ecossistema Angular, e as camadas exigem disciplina no começo.
