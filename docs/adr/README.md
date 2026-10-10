# Architecture Decision Records

Um ADR registra **uma** decisão técnica: o contexto, o que foi decidido, as alternativas e as consequências.
Formato MADR enxuto ([template.md](template.md)). Para criar um novo, use a skill `adr`.

## Regras
- Numeração sequencial de 4 dígitos. O número nunca é reutilizado.
- ADR aceito não se edita. Para mudar a decisão, crie um novo ADR e marque o antigo como `Substituído por NNNN`.
- Correções de texto (typo, link) podem ser feitas direto.
- Status: `Proposto` → `Aceito` ou `Rejeitado`. Depois, opcionalmente, `Substituído por NNNN`.

## Índice
| # | Título | Status |
|---|---|---|
| [0001](0001-registrar-decisoes-em-adrs.md) | Registrar decisões em ADRs | Aceito |
| [0002](0002-fluxo-spec-driven-com-ia.md) | Fluxo spec-driven com IA | Aceito |
| [0003](0003-backend-clean-architecture.md) | Backend em Clean Architecture | Aceito |
| [0004](0004-repository-unit-of-work-paginacao.md) | Repository, Unit of Work e PagedResult | Aceito |
| [0005](0005-postgresql-ef-core.md) | PostgreSQL + EF Core | Aceito |
| [0006](0006-validacao-fluentvalidation-problemdetails.md) | Validação com FluentValidation e erros como ProblemDetails | Aceito |
| [0007](0007-dtos-com-mapeamento-manual.md) | DTOs com mapeamento manual | Aceito |
| [0008](0008-api-rest-com-controllers.md) | API REST com Controllers | Aceito |
| [0009](0009-frontend-angular-fsd-ng-zorro.md) | Frontend Angular com Feature-Sliced Design e NG-ZORRO | Aceito |
| [0010](0010-estrategia-de-testes.md) | Estratégia de testes | Aceito |
| [0011](0011-monorepo.md) | Monorepo backend + frontend | Aceito |
| [0012](0012-autorizacao-por-permissao.md) | Autorização por permissão com HasPermission | Aceito |
| [0013](0013-migrations-automaticas-na-inicializacao.md) | Migrations aplicadas automaticamente na inicialização | Aceito |
