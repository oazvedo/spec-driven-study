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
- [x] **T09. `IUsuarioService` / `UsuarioService` + registro no `ApplicationModule`**
  - Cobre: CA-03, CA-04, CA-06, CA-07, CA-08 e CA-09 (unidade)
  - Testes: criar, atualizar, ativar e inativar com sucesso (com `CommitAsync`); **falha:** `ValidationException` sem commit,
    `ConflictException` e `NotFoundException`
  - Verificação: `dotnet test --project backend/tests/Template.UnitTests --filter-class "*UsuarioServiceTests"`
- [x] **T10. `UsuariosController` + testes funcionais**
  - Cobre: CA-01 a CA-09 (funcional)
  - Testes: as linhas "Funcional" da tabela CA → testes do `plan.md`
  - Verificação: `dotnet test backend` todo verde; os 6 endpoints aparecem e respondem pelo `/scalar`
- [x] **T10b. Violação de unicidade concorrente vira 409** (descoberta na simplify da T09)
  - Problema: duas requisições com o mesmo e-mail passam juntas pelo `ExisteEmailAsync`; a segunda estoura o índice
    único no `CommitAsync` como `DbUpdateException` → 500 em vez de 409 (CA-06)
  - Arquivos: `Infrastructure/Data/UnitOfWork.cs` traduz `PostgresException` com SqlState `23505` (unique_violation)
    em `ConflictException`. Vale para todas as entidades; a Application continua sem conhecer EF/Npgsql
  - Testes: integração: commit com e-mail duplicado (sem passar pelo service) → `ConflictException`
    (substitui o teste `Commit_EmailDuplicado_LancaDbUpdateException` da T07)
  - Verificação: `dotnet test backend`

## Fase 3: Base do frontend
- [x] **T11. App Angular + NG-ZORRO + layout + proxy**
  - Arquivos: `frontend/` (ng new), `proxy.conf.json`, shell em `app/app.ts`, as pastas FSD com `.gitkeep`
  - Verificação: `npm run build`; `npm start` abre o layout com o menu
- [x] **T12. Lint com as regras do FSD**
  - Feito: `eslint.config.js` (angular-eslint + `eslint-plugin-boundaries` v7) e `fsd.lint.test.mjs`, um teste permanente
    com sucesso e falha que roda dentro do `npm run lint` (substitui a verificação manual com import temporário)
  - Arquivos: `eslint.config.js` com `eslint-plugin-boundaries`
  - Testes: **falha:** um import proibido criado temporariamente (`entities` → `features`) faz `npm run lint` falhar; remova-o em seguida
  - Verificação: `npm run lint` verde
- [x] **T13. `shared/api`**
  - Arquivos: `paged-result.ts`, `problem-details.ts`, `problem-details.interceptor.ts`, `apply-server-errors.ts`
  - Testes: `applyServerErrors` marca o controle certo; **falha:** campo desconhecido no `errors` é ignorado sem quebrar
  - Verificação: `npm test`

## Fase 4: Usuario no frontend (skill `angular-crud`)
- [x] **T14. `entities/usuario`** (model, api, status tag)
  - Verificação: `npm test` (URL e verbo de cada método)
- [x] **T15. `features/usuario-form`**
  - Cobre: CA-03, CA-04, CA-05, CA-06 e CA-07 (frontend unit)
  - Testes: válido chama `criar` e emite `salvo`; **falha:** vazio não chama a API; um 400 ou 409 do servidor exibe o erro no campo
  - Incluir (descoberto na simplify da T13): `HttpContextToken` em `shared/api` (ex.: `STATUS_TRATADOS`) para quem chama
    declarar os status que trata; o interceptor não mostra nz-message para eles. O form declara 409, para o erro de
    e-mail duplicado não aparecer duas vezes (no campo e no nz-message)
  - Verificação: `npm test`
- [x] **T16. `features/usuario-toggle-ativo`**
  - Cobre: CA-09
  - Verificação: `npm test`
- [x] **T17. `widgets/usuarios-table`**
  - Cobre: CA-01
  - Testes: troca de página chama `listar(page, pageSize)`
  - Verificação: `npm test`
- [x] **T18. `pages/usuarios-page` + rota lazy + item de menu** (teste manual na tela ainda não feito; o e2e da T19 cobre o mesmo roteiro)
  - Verificação: `npm run build && npm run lint`; teste manual: cadastrar, editar e inativar pela tela

## Fase 5: Ponta a ponta e fechamento
- [x] **T19. Playwright + e2e de usuários**
  - Feito: o `playwright.config.ts` sobe a API e o `ng serve` (ou reaproveita os que já estão rodando).
    No WSL, o Chromium precisa das libs do sistema, instaladas uma vez com `sudo env "PATH=$PATH" npx playwright install-deps chromium`
  - Cobre: CA-03 e CA-04 (e2e)
  - Testes: cadastrar e ver na tabela; **falha:** formulário vazio mostra erros e não cria nada
  - Verificação: `npm run e2e` (com `docker compose up -d` e a API rodando)
- [ ] **T20. Fechamento**
  - Atualizar a seção de comandos do `README.md`, rodar a skill `code-review` na spec, conferir a cobertura de todos os CAs,
    marcar a spec como `Concluída` e registrar com a skill `study-log`
  - Verificação: `dotnet test backend && cd frontend && npm run lint && npm test && npm run e2e`
  - Feito até aqui: README, code-review, cobertura dos CAs (plan.md atualizado com o teste da T10b) e study-log.
    Falta marcar a spec como `Concluída`, depois das T21 a T24

## Fase 6: Ajustes do code-review (descobertos na T20)
- [x] **T21. Paginação sem overflow**
  - Problema: `page=2147483647` estoura `int` em `(page - 1) * pageSize` → OFFSET negativo → 500
  - Feito: o `RepositoryBase` calcula o deslocamento em `long`. A página além do fim volta vazia (200) sem a segunda consulta.
    O achado "cancelamento vira 500" era falso: o `UseExceptionHandler` do .NET 8+ já responde 499 e só loga em Debug
    quando o cliente aborta (comentário no `GlobalExceptionHandler`)
  - Testes: funcional `Listar_PaginaMaxima_Retorna200Vazio` (falha sem a correção)
  - Verificação: `dotnet test backend`
- [x] **T22. Erros de binding em português (ADR 0006)**
  - Problema: `GET /api/usuarios?page=abc` → 400 com título e mensagem em inglês
  - Feito: `Api/ErrorHandling/MensagensDeBinding.cs` traduz o `ModelBindingMessageProvider` e monta o 400 do `[ApiController]`
    (`InvalidModelStateResponseFactory`) com o mesmo título do `GlobalExceptionHandler`. JSON malformado vira
    "O JSON enviado não é válido.". Sem o `[Required]` implícito: campo ausente chega ao FluentValidation (mensagem em PT)
  - Testes: funcional: `page=abc` → 400 com mensagem em português; **falha:** JSON malformado → 400 em português
  - Verificação: `dotnet test backend`
- [x] **T23. `usuario-form`: sem envio duplo e com retorno em todo 400**
  - Problema: Enter durante o salvamento manda um segundo POST/PUT; um 400 sem `nome`/`email` no `errors`
    não mostra nada (o interceptor fica quieto e o form ignora as chaves)
  - Feito: guarda `if (this.salvando()) return;`. `applyServerErrors` devolve as mensagens sem campo e o form as mostra
    no nz-message. O padrão de `STATUS_TRATADOS` passou de `[400]` para `[]`: os 400 de chamadas sem formulário
    voltam a aparecer. Skill `angular-crud` atualizada com as duas regras
  - Testes: salvar com uma requisição em andamento não chama a API de novo; **falha:** 400 sem campo conhecido mostra uma mensagem
  - Verificação: `npm test`
- [x] **T24. Depois de cadastrar, a tabela volta para a página 1**
  - Problema: a listagem mostra os mais recentes primeiro (RN-05); quem está na página 3 cadastra e não vê o novo usuário
  - Feito: `UsuariosTable.irParaPrimeiraPagina()`; a página usa esse método ao criar e `recarregar()` ao editar.
    Regra registrada na skill `angular-crud`
  - Testes: `irParaPrimeiraPagina` leva a tabela à página 1 (e recarrega se já estiver nela); `recarregar` mantém a página
  - Verificação: `npm test && npm run e2e`
