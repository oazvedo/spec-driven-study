# 001. Fundação: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Os testes vão junto com o código que eles cobrem.
> Implementar com a skill `implement-task`, uma por vez.

## Fase 1: Base do backend
- [x] **T01. Solution e projetos** (o `docker-compose.yml` já existe na raiz)
  - Arquivos: `backend/Template.sln`, `backend/Directory.Build.props`,
    `backend/Directory.Packages.props`, os 4 projetos em `src/` com as referências do ADR 0003
  - Verificação: `docker compose up -d` sobe o Postgres; `dotnet build backend` sem erros
- [x] **T02. Projetos de teste + testes de arquitetura**
  - Arquivos: `backend/tests/Template.{UnitTests,IntegrationTests,FunctionalTests,ArchitectureTests}`, `backend/tests/Directory.Build.props`, `global.json` (Microsoft.Testing.Platform)
  - Cobre: ADR 0003 (Domain não depende de nada; Application não depende de Infrastructure nem de Api)
  - Verificação: `dotnet test backend` → testes de arquitetura verdes
- [x] **T03. Domain comum**
  - Arquivos: `EntityBase`, `PagedResult`, `NotFoundException`, `ConflictException`, `IRepositoryBase<T>`, `IUnitOfWork`
  - Testes: `PagedResult` (TotalPages, HasNext/HasPrevious, Map; `PageSize = 0` → `TotalPages = 0`); `EntityBase` nasce ativo, `Inativar()`/`Ativar()`
  - Verificação: `dotnet test backend/tests/Template.UnitTests`
- [x] **T04. Infrastructure comum**
  - Arquivos: `AppDbContext`, `UnitOfWork`, `RepositoryBase<T>`, `InfraModule`
  - Verificação: `dotnet build backend`
- [x] **T05. Api comum: migrations automáticas, Scalar, health check e fixture de testes funcionais**
  - Arquivos: `Program.cs`, `GlobalExceptionHandler`, `MigrationExtensions` (ADR 0013), `ApplicationModule` (vazio),
    `appsettings.Development.json`, `launchSettings.json` (abre `/scalar` no debug); fixture Testcontainers + `WebApplicationFactory`
  - Cobre: CA-10
  - Testes: `Health_BancoDisponivel_Retorna200`; **falha:** `GlobalExceptionHandler` converte `NotFoundException` em 404 ProblemDetails (unidade)
  - Verificação: `dotnet test backend`; com `docker compose up -d`, o `dotnet run --project backend/src/Template.Api`
    sobe sem erro, cria o banco `template_crud` (tabela `__EFMigrationsHistory`) e `/scalar` abre a documentação

## Fase 2: Usuario no backend (skill `dotnet-crud`)
- [x] **T06. Entidade `Usuario` + `IUsuarioRepository`**
  - Cobre: CA-09 (unidade)
  - Verificação: `dotnet test --project backend/tests/Template.UnitTests --filter-class "*UsuarioTests"`
- [x] **T07. `UsuarioConfig`, `UsuarioRepository`, registro no `InfraModule` + migration `AdicionarUsuario`**
  - Cobre: CA-01 e CA-06 (integração)
  - Testes: salvar e paginar 25 registros; `Remover` + commit apaga o registro (ADR 0015);
    `ExisteEmailAsync` encontra o e-mail digitado com outra caixa ou com espaços (o repositório normaliza; RN-02);
    `CreatedAt` preenchido ao adicionar; ao atualizar, `UpdatedAt` preenchido e `CreatedAt` inalterado (`AppDbContext`, T04);
    **falha:** e-mail duplicado → `DbUpdateException`
  - Verificação: `dotnet test backend/tests/Template.IntegrationTests`; `dotnet run` aplica a migration e a tabela `usuarios` passa a existir
- [x] **T08. DTOs, mapeamento e validators**
  - Cobre: CA-04 e CA-05 (unidade)
  - Testes: validator aceita um request válido; **falha:** nome vazio, nome com 151 caracteres, e-mail vazio, e-mail inválido
  - Verificação: `dotnet test --project backend/tests/Template.UnitTests --filter-class "*ValidatorTests"`
- [ ] **T09. `IUsuarioService` / `UsuarioService` + registro no `ApplicationModule`**
  - Cobre: CA-03, CA-04, CA-06, CA-07, CA-08 e CA-09 (unidade)
  - Testes: criar, atualizar, ativar e inativar com sucesso (com `CommitAsync`); **falha:** `ValidationException` sem commit,
    `ConflictException` e `NotFoundException`
  - Verificação: `dotnet test --project backend/tests/Template.UnitTests --filter-class "*UsuarioServiceTests"`
- [ ] **T10. `UsuariosController` + testes funcionais**
  - Cobre: CA-01 a CA-09 (funcional)
  - Testes: as linhas "Funcional" da tabela CA → testes do `plan.md`
  - Verificação: `dotnet test backend` todo verde; os 6 endpoints aparecem e respondem pelo `/scalar`

## Fase 3: Base do frontend
- [ ] **T11. App Angular + NG-ZORRO + layout + proxy**
  - Arquivos: `frontend/` (ng new), `proxy.conf.json`, `app/layout/`, as pastas FSD com `.gitkeep`
  - Verificação: `npm run build`; `npm start` abre o layout com o menu
- [ ] **T12. Lint com as regras do FSD**
  - Arquivos: `eslint.config.js` com `eslint-plugin-boundaries`
  - Testes: **falha:** um import proibido criado temporariamente (`entities` → `features`) faz `npm run lint` falhar; remova-o em seguida
  - Verificação: `npm run lint` verde
- [ ] **T13. `shared/api`**
  - Arquivos: `paged-result.ts`, `problem-details.ts`, `problem-details.interceptor.ts`, `apply-server-errors.ts`
  - Testes: `applyServerErrors` marca o controle certo; **falha:** campo desconhecido no `errors` é ignorado sem quebrar
  - Verificação: `npm test`

## Fase 4: Usuario no frontend (skill `angular-crud`)
- [ ] **T14. `entities/usuario`** (model, api, status tag)
  - Verificação: `npm test` (URL e verbo de cada método)
- [ ] **T15. `features/usuario-form`**
  - Cobre: CA-03, CA-04, CA-05, CA-06 e CA-07 (frontend unit)
  - Testes: válido chama `criar` e emite `salvo`; **falha:** vazio não chama a API; um 400 ou 409 do servidor exibe o erro no campo
  - Verificação: `npm test`
- [ ] **T16. `features/usuario-toggle-ativo`**
  - Cobre: CA-09
  - Verificação: `npm test`
- [ ] **T17. `widgets/usuarios-table`**
  - Cobre: CA-01
  - Testes: troca de página chama `listar(page, pageSize)`
  - Verificação: `npm test`
- [ ] **T18. `pages/usuarios-page` + rota lazy + item de menu**
  - Verificação: `npm run build && npm run lint`; teste manual: cadastrar, editar e inativar pela tela

## Fase 5: Ponta a ponta e fechamento
- [ ] **T19. Playwright + e2e de usuários**
  - Cobre: CA-03 e CA-04 (e2e)
  - Testes: cadastrar e ver na tabela; **falha:** formulário vazio mostra erros e não cria nada
  - Verificação: `npm run e2e` (com `docker compose up -d` e a API rodando)
- [ ] **T20. Fechamento**
  - Atualizar a seção de comandos do `README.md`, rodar a skill `code-review` na spec, conferir a cobertura de todos os CAs,
    marcar a spec como `Concluída` e registrar com a skill `study-log`
  - Verificação: `dotnet test backend && cd frontend && npm run lint && npm test && npm run e2e`
