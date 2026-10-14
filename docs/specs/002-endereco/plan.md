# 002. Endereço: Plan

- **Spec:** [spec.md](spec.md)
- **ADRs aplicados:** 0003, 0004, 0005, 0006, 0007, 0008, 0009, 0010, 0013, 0015
- **ADRs novos propostos:** [0016](../../adr/0016-delete-na-api-para-entidades-sem-historico.md) (DELETE na API para entidades sem histórico)

## Abordagem
O CRUD de `Endereco` já existe no backend (entidade, config, migration `AddEndereco`, repositório, DTOs,
validators, service e controller). Esta spec **ajusta** esse código aos ADRs, adiciona os testes que faltam
e cria o frontend copiando o padrão de `usuario` (skill `angular-crud`).
Ajustes no backend: devolver `ativo` e `createdAt` na resposta, criar `Ativar`, responder 204 em
`inativar`/`excluir`, renomear `Deletar` para `Excluir` (ADR 0016) e deixar o `OnDelete` explícito (ADR 0015).
O `Remover` está justificado aqui: endereço não tem valor histórico (spec, RN-08).

## Modelo de dados
| Entidade | Campo | Tipo | Regras |
|---|---|---|---|
| (EntityBase) | `Id`, `CreatedAt`, `UpdatedAt`, `Ativo` | — | igual à spec 001 (RN-07) |
| `Endereco` | `UsuarioId` | `Guid` | FK → `usuarios`, obrigatório, índice (RN-01) |
| `Endereco` | `Logradouro` | `string(100)` | obrigatório (RN-02, RN-03) |
| `Endereco` | `Numero` | `string(10)` | obrigatório |
| `Endereco` | `Complemento` | `string(100)?` | opcional |
| `Endereco` | `Bairro`, `Cidade` | `string(100)` | obrigatórios |
| `Endereco` | `Estado` | `char(2)` | obrigatório, `^[A-Z]{2}$` (RN-04) |
| `Endereco` | `Cep` | `string(9)` | obrigatório, `^\d{5}-?\d{3}$` (RN-05) |
| `Endereco` | `Tipo` | `int` (`EnderecoTipo`) | `Comercial=1`, `Residencial=2`, `Entrega=3` (RN-06) |

Tabela `enderecos`, já criada pela migration `AddEndereco`. A FK passa a declarar `OnDelete(DeleteBehavior.Cascade)`
na `EnderecoConfig`, que é o comportamento que já está na migration: o modelo não muda, então não há migration nova.
(O `Usuario` nunca é removido, então a cascata nunca dispara na prática.)

## Contrato da API
| Verbo | Rota | Request | Sucesso | Erros |
|---|---|---|---|---|
| GET | `/api/enderecos?page&pageSize` | — | 200 `PagedResult<EnderecoResponse>` | 400 |
| GET | `/api/enderecos/{id:guid}` | — | 200 `EnderecoResponse` | 404 |
| POST | `/api/enderecos` | `EnderecoRequest` | 201 + `Location` | 400, 404 (usuário) |
| PUT | `/api/enderecos/{id:guid}` | `EnderecoRequest` | 200 `EnderecoResponse` | 400, 404 |
| PATCH | `/api/enderecos/{id:guid}/ativar` | — | 204 | 404 |
| PATCH | `/api/enderecos/{id:guid}/inativar` | — | 204 | 404 |
| DELETE | `/api/enderecos/{id:guid}` | — | 204 | 404 |

`EnderecoRequest = { usuarioId, logradouro, numero, complemento?, bairro, cidade, estado, cep, tipo }` (tipo numérico).
`EnderecoResponse = { id, usuarioId, usuarioNome, logradouro, numero, complemento, bairro, cidade, estado, cep, tipo, ativo, createdAt }`.
O `usuarioNome` vem de `IUsuarioRepository.ObterNomesAsync(ids)`, uma consulta por página (T08b).
Erros em ProblemDetails (ADR 0006).

## Backend: arquivos
**Alterados:**
- `Template.Infrastructure/Data/Configurations/EnderecoConfig.cs`: `OnDelete(DeleteBehavior.Cascade)` explícito.
- `Template.Application/Dtos/Enderecos/EnderecoDtos.cs`: `Ativo` e `CreatedAt` no `EnderecoResponse` e no `ToResponse`.
- `Template.Application/Services/Interfaces/IEnderecoService.cs` e `Services/EnderecoService.cs`:
  `AtivarAsync` novo; `DeletarAsync` → `ExcluirAsync`.
- `Template.Api/Controllers/EnderecosController.cs`: `Ativar`, `Inativar` e `Excluir` devolvendo `NoContent()`.

**Testes novos:**
- `Template.UnitTests/Application/EnderecoServiceTests.cs` e `EnderecoValidatorsTests.cs`
- `Template.IntegrationTests/Repositories/EnderecoRepositoryTests.cs`
- `Template.FunctionalTests/EnderecosTests.cs`

## Frontend: arquivos
- **`entities/endereco/`** (renomeia a pasta vazia `entities/enderecos`, porque a slice de entidade fica no singular, como `usuario`):
  `model/endereco.model.ts` (`Endereco`, `EnderecoRequest`, `EnderecoTipo` + rótulos), `api/endereco.api.ts`
  (7 métodos, incluindo `excluir`), `index.ts`. O status usa `AtivoTag` de `shared/ui` (T05b).
- **`features/endereco-form/`**: criar e editar. Seletor de usuário com `nz-select`, carregado por `UsuarioApi.listar(1, 100)`
  (importa `entities/usuario`). Tipo com `nz-select`. Validações espelham o validator. `STATUS_TRATADOS = [400, 404]`.
- **`features/endereco-toggle-ativo/`**: igual a `usuario-toggle-ativo`.
- **`features/endereco-excluir/`**: botão "Excluir" com `nz-popconfirm`; emite `excluido = output<void>()`.
- **`widgets/enderecos-table/`**: `nz-table` com paginação no servidor. Colunas: usuário, logradouro e número, cidade/UF,
  CEP, tipo, status e ações (editar, toggle, excluir). O nome do usuário vem no próprio `usuarioNome` da API (T08b).
- **`pages/enderecos-page/`**: tabela + form em `nz-modal`. A rota `enderecos` já está em `app.routes.ts`, mas aponta
  para uma página que ainda não existe; falta o item no `menu` de `app/app.ts`.
- `ng-zorro.less`: `select` (e qualquer outro componente ainda não importado).
- `e2e/enderecos.spec.ts`.

## Critérios de aceite → testes
| CA | Nível | Teste (nome sugerido) |
|---|---|---|
| CA-01 | Integração | `ListarPaginadoAsync_25Enderecos_RetornaPrimeiraPaginaComTotal` |
| CA-01 | Funcional | `Listar_SemParametros_Retorna20ItensETotal` |
| CA-02 | Funcional | `Listar_ParametrosForaDoLimite_Retorna400` (`pageSize=101` e `page=0`) |
| CA-03 | Unidade | `CriarAsync_RequestValido_AdicionaECommita` |
| CA-03 | Funcional | `Criar_RequestValido_Retorna201ComLocationEAtivo` |
| CA-03 | E2E | `cadastrar endereço e vê-lo na tabela` |
| CA-04 | Unidade | `EnderecoValidator_CampoObrigatorioVazio_Falha` · `EnderecoValidator_AcimaDoMaximo_Falha` · `CriarAsync_RequestInvalido_LancaValidationExceptionSemCommit` |
| CA-04 | Funcional | `Criar_CamposVazios_Retorna400ComErrosPorCampo` |
| CA-04 | Frontend unit | `endereco-form: form vazio não chama a API` |
| CA-04 | E2E | `submeter formulário vazio mostra erros e não cria` |
| CA-05 | Unidade | `EnderecoValidator_UfInvalida_Falha` · `EnderecoValidator_CepInvalido_Falha` · `EnderecoValidator_TipoForaDoEnum_Falha` · `EnderecoValidator_CepComESemHifen_Passa` |
| CA-06 | Unidade | `CriarAsync_UsuarioInexistente_LancaNotFoundSemCommit` · `AtualizarAsync_TrocaParaUsuarioInexistente_LancaNotFound` |
| CA-06 | Funcional | `Criar_UsuarioInexistente_Retorna404` |
| CA-07 | Funcional | `Atualizar_RequestValido_Retorna200ComDadosNovos` (inclui trocar o usuário) |
| CA-08 | Unidade | `ObterAsync_IdInexistente_LancaNotFoundException` |
| CA-08 | Funcional | `Obter_IdInexistente_Retorna404` · `Atualizar_IdInexistente_Retorna404` · `AtivarInativarExcluir_IdInexistente_Retorna404` |
| CA-09 | Funcional | `InativarEAtivar_EnderecoExistente_Retorna204EAlteraStatus` |
| CA-10 | Unidade | `ExcluirAsync_EnderecoExistente_RemoveECommita` |
| CA-10 | Integração | `Remover_Commit_EnderecoNaoExisteMais` |
| CA-10 | Funcional | `Excluir_EnderecoExistente_Retorna204EDepois404` |
| CA-10 | E2E | `excluir endereço o remove da tabela` |
| CA-11 | Frontend unit | `endereco-excluir: cancelar a confirmação não chama a API` |

## Riscos e decisões pendentes
- **Lista de usuários limitada a 100** (máximo do `pageSize`, RN-05 da spec 001). Com mais de 100 usuários, o seletor do
  formulário fica incompleto (o nome na tabela vem da API desde a T08b). Aceito para esta spec; a busca de usuário no seletor entra junto com os filtros (fora de escopo).
- **Tipo numérico no JSON:** a API não usa `JsonStringEnumConverter`, então `tipo` trafega como `1`, `2` ou `3`. O frontend
  guarda os rótulos em `endereco.model.ts`. Mudar para string seria uma decisão global (afeta todos os enums) e não entra aqui.
- **Frontend quebrado hoje:** a rota `enderecos` importa `../pages/enderecos-page`, que não existe. O build só volta a passar na task da página;
  as tasks anteriores do frontend verificam com `npm test` das slices e `npm run lint`.
- **Docker obrigatório** para os testes de integração, funcionais e e2e.
