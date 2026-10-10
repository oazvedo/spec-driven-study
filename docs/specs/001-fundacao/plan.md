# 001. Fundação: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0003, 0004, 0005, 0006, 0007, 0008, 0009, 0010, 0011, 0013
- **ADRs novos propostos:** nenhum

## Abordagem
Esta spec cria, uma única vez, a **infraestrutura comum** dos dois lados e, sobre ela, o CRUD de `Usuario`
seguindo exatamente as skills `dotnet-crud` e `angular-crud`. O `Usuario` vira a implementação de referência.
O ambiente local é composto por Postgres via `docker compose`, a API em `dotnet run` e o Angular em `ng serve` com proxy `/api`.

## Modelo de dados
| Entidade | Campo | Tipo | Regras |
|---|---|---|---|
| (EntityBase) | `Id` | `Guid` (v7) | PK |
| (EntityBase) | `CreatedAt` / `UpdatedAt` | `DateTime` UTC / `DateTime?` | preenchidos no `SaveChangesAsync` |
| (EntityBase) | `Ativo` | `bool` | padrão `true` (RN-03, RN-04) |
| `Usuario` | `Nome` | `string(150)` | obrigatório (RN-01) |
| `Usuario` | `Email` | `string(254)` | obrigatório, índice único (RN-02) |

Tabela `usuarios`, colunas em `snake_case`.

## Contrato da API
| Verbo | Rota | Request | Sucesso | Erros |
|---|---|---|---|---|
| GET | `/api/usuarios?page&pageSize` | — | 200 `PagedResult<UsuarioResponse>` | 400 |
| GET | `/api/usuarios/{id:guid}` | — | 200 `UsuarioResponse` | 404 |
| POST | `/api/usuarios` | `{ nome, email }` | 201 + `Location` | 400, 409 |
| PUT | `/api/usuarios/{id:guid}` | `{ nome, email }` | 200 `UsuarioResponse` | 400, 404, 409 |
| PATCH | `/api/usuarios/{id:guid}/ativar` | — | 204 | 404 |
| PATCH | `/api/usuarios/{id:guid}/inativar` | — | 204 | 404 |
| GET | `/api/health` | — | 200 `Healthy` | 503 |
| GET | `/scalar` (só Development) | — | interface de documentação da API | — |

`UsuarioResponse = { id, nome, email, ativo, createdAt }`. Erros em ProblemDetails (ADR 0006).

## Backend: arquivos
**Solution:** `backend/Template.sln` com `src/` (4 projetos) e `tests/` (4 projetos).
Usar `<TargetFramework>net10.0</TargetFramework>`, `Nullable` e `ImplicitUsings` habilitados
(via `backend/Directory.Build.props`) e versões centralizadas em `backend/Directory.Packages.props`.

**Infraestrutura comum (só nesta spec):**
- Domain: `Common/EntityBase.cs`, `Common/PagedResult.cs`, `Common/Exceptions/{NotFoundException,ConflictException}.cs`,
  `Entities/Interfaces/{IRepositoryBase,IUnitOfWork}.cs`
- Infrastructure: `Data/AppDbContext.cs` (`ApplyConfigurationsFromAssembly`, datas no `SaveChangesAsync`),
  `Data/UnitOfWork.cs`, `Repositories/RepositoryBase.cs` (`ListarPaginadoAsync` com `AsNoTracking`,
  ordenado por `CreatedAt` desc; `Remover` via `Set.Remove`, ADR 0015), `InfraModule.cs`
- Application: `ApplicationModule.cs`
- Infrastructure: `Data/MigrationExtensions.cs` (`ApplyMigrationsAsync`, ADR 0013)
- Api: `Program.cs` (`AddApplication().AddInfra()`, `AddProblemDetails`, `AddExceptionHandler<GlobalExceptionHandler>`,
  `AddControllers`, `AddOpenApi` (o health check do banco é registrado no `InfraModule`);
  depois do `Build()`: `await app.Services.ApplyMigrationsAsync()`, `MapOpenApi()` + `MapScalarApiReference()` em Development
  e `MapHealthChecks("/api/health")`), `ErrorHandling/GlobalExceptionHandler.cs`,
  `appsettings.Development.json` (`ConnectionStrings:Default` =
  `Host=localhost;Port=5432;Database=template_crud;Username=postgres;Password=postgres`, igual ao `docker-compose.yml`)
- Api: no .NET 10 o `Program` gerado já é público, então a `WebApplicationFactory<Program>` funciona sem `public partial class Program;`

**Usuario:** os arquivos dos passos 1 a 10 da skill `dotnet-crud`.

**Pacotes:**
- Infrastructure: `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions`, `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`
- Application: `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`
- Api: `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`, `Microsoft.EntityFrameworkCore.Design` (o `dotnet ef` exige no projeto de inicialização; ferramenta `dotnet-ef` local em `dotnet-tools.json`)
- Testes: `xunit.v3`, `NSubstitute`, `AwesomeAssertions` (ADR 0014), `Testcontainers.PostgreSql`,
  `Microsoft.AspNetCore.Mvc.Testing`, `NetArchTest.Rules`

**Testes de integração e funcionais:** uma fixture compartilhada (`IAsyncLifetime`) sobe o Postgres via Testcontainers.
Nos testes de integração, a fixture aplica as migrations (`ApplyMigrationsAsync`). Nos funcionais, a
`WebApplicationFactory<Program>` troca a connection string pela do container, e a própria API aplica as migrations ao subir (ADR 0013).

## Frontend: arquivos
- `ng new frontend --routing --style=scss --ssr=false` e depois `ng add ng-zorro-antd` (locale pt-BR, ícones).
- `proxy.conf.json`: `/api` → `http://localhost:5000` (porta da API definida em `launchSettings.json`).
- Estrutura FSD em `src/app/`: `app/`, `pages/`, `widgets/`, `features/`, `entities/` e `shared/` (ADR 0009).
- `app/`: `app.config.ts` (`provideHttpClient(withInterceptors([problemDetailsInterceptor]))`, NG-ZORRO pt-BR),
  `app.routes.ts` (redireciona `''` → `usuarios`) e `layout/` (`nz-layout` + `nz-menu` lateral).
- `shared/api/`: `paged-result.ts`, `problem-details.ts` (tipo), `problem-details.interceptor.ts` (`nz-message` para erros ≠ 400)
  e `apply-server-errors.ts` (aplica `errors` do 400 em um `FormGroup`).
- Lint: `ng add @angular-eslint/schematics` + `eslint-plugin-boundaries`, com os elementos `app`, `pages`, `widgets`,
  `features`, `entities` e `shared` e a regra "só importa camadas abaixo".
- Testes: Vitest (padrão do Angular) e Playwright em `frontend/e2e/` (`npm run e2e`).
- **Usuario:** os passos 1 a 4 da skill `angular-crud`.

## Critérios de aceite → testes
| CA | Nível | Teste (nome sugerido) |
|---|---|---|
| CA-01 | Integração | `ListarPaginadoAsync_25Registros_RetornaPrimeiraPaginaComTotal` |
| CA-01 | Funcional | `Listar_SemParametros_Retorna20ItensETotal` |
| CA-02 | Funcional | `Listar_PageSizeAcimaDe100_Retorna400` · `Listar_PageZero_Retorna400` |
| CA-03 | Unidade | `CriarAsync_RequestValido_AdicionaECommita` |
| CA-03 | Funcional | `Criar_RequestValido_Retorna201ComLocation` |
| CA-03 | E2E | `cadastrar usuário e vê-lo na tabela` |
| CA-04 | Unidade | `CriarAsync_NomeVazio_LancaValidationExceptionSemCommit` |
| CA-04 | Funcional | `Criar_CamposVazios_Retorna400ComErrosPorCampo` |
| CA-04 | Frontend unit | `usuario-form: form vazio não chama a API` |
| CA-04 | E2E | `submeter formulário vazio mostra erros e não cria` |
| CA-05 | Unidade | `CriarUsuarioRequestValidator_EmailInvalido_Falha` |
| CA-06 | Unidade | `CriarAsync_EmailDuplicado_LancaConflictException` · `Usuario_EmailComMaiusculas_GuardaEmMinusculas` |
| CA-06 | Integração | `Commit_EmailDuplicado_LancaDbUpdateException` |
| CA-06 | Funcional | `Criar_EmailDuplicado_Retorna409` · `Criar_EmailDuplicadoComOutraCaixa_Retorna409` |
| CA-07 | Funcional | `Atualizar_RequestValido_Retorna200ComDadosNovos` |
| CA-08 | Unidade | `ObterAsync_IdInexistente_LancaNotFoundException` |
| CA-08 | Funcional | `Obter_IdInexistente_Retorna404` · `Inativar_IdInexistente_Retorna404` |
| CA-09 | Unidade | `Usuario_Inativar_DefineAtivoFalse` |
| CA-09 | Funcional | `Inativar_UsuarioExistente_Retorna204EStatusInativo` |
| CA-10 | Funcional | `Health_BancoDisponivel_Retorna200` |
| ADR 0003 | Arquitetura | `Domain_NaoDependeDeOutrasCamadas` · `Application_NaoDependeDeInfrastructure` |

## Riscos e decisões pendentes
- **Unicidade do e-mail (RN-02):** a entidade `Usuario` normaliza o e-mail com `Trim().ToLowerInvariant()`
  no construtor e no `Atualizar`, e o `UsuarioRepository.ExisteEmailAsync` normaliza o e-mail recebido (o service passa o e-mail cru). Assim o índice
  único comum já garante a regra, sem precisar de `citext` nem de um índice por expressão.
- **Docker obrigatório** para os testes de integração, funcionais e e2e.
- **Versões:** usar as versões estáveis mais recentes do Angular, do NG-ZORRO e dos pacotes NuGet no momento da implementação.
  Se alguma API citada aqui tiver mudado (por exemplo `rxResource`), siga a documentação oficial e anote no study-log.
