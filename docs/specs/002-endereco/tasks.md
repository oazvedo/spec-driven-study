# 002. Endereço: Tasks

- **Spec:** [spec.md](spec.md) · **Plan:** [plan.md](plan.md)

> Uma task = um objetivo, ~30 min, verificável. Os testes vão junto com o código que eles cobrem.
> Implementar com a skill `implement-task`, uma por vez.
> O CRUD do backend já existe: as tasks da Fase 1 ajustam esse código aos ADRs e acrescentam os testes.

## Fase 1: Endereco no backend (skill `dotnet-crud`)
- [x] **T01. `EnderecoConfig` com `OnDelete` explícito + testes de integração do repositório**
  (descoberto: as classes rodavam em paralelo no mesmo banco; paralelização desligada em `Shared/BancoDeTeste.cs`)
  - Arquivos: `Template.Infrastructure/Data/Configurations/EnderecoConfig.cs`,
    `Template.IntegrationTests/Repositories/EnderecoRepositoryTests.cs`
  - Cobre: CA-01 e CA-10 (integração)
  - Testes: salvar e paginar 25 endereços (total e 2 páginas); `Remover` + commit apaga o registro.
    **Falha:** salvar com um `UsuarioId` inexistente lança `DbUpdateException` (FK)
  - Verificação: `dotnet ef migrations has-pending-model-changes --project backend/src/Template.Infrastructure --startup-project backend/src/Template.Api`
    → sem mudanças; `dotnet test --project backend/tests/Template.IntegrationTests --filter-class "*EnderecoRepositoryTests"`
- [x] **T02. Testes dos validators de endereço**
  - Arquivos: `Template.UnitTests/Application/Validators/EnderecoValidatorTests.cs` (o validator já existe)
  - Cobre: CA-04 e CA-05 (unidade)
  - Testes: request válido passa; CEP com e sem hífen passa. **Falha:** cada obrigatório vazio; acima do máximo;
    UF `sp` e `SPX`; CEP `1234-567`; tipo `99`; `UsuarioId` vazio
  - Verificação: `dotnet test --project backend/tests/Template.UnitTests --filter-class "*EnderecoRequestValidatorTests"`
- [x] **T03. `EnderecoResponse` com `ativo` e `createdAt`, `AtivarAsync` e `DeletarAsync` → `ExcluirAsync` + testes do service**
  - Arquivos: `Template.Application/Dtos/Enderecos/EnderecoDtos.cs`, `Services/Interfaces/IEnderecoService.cs`,
    `Services/EnderecoService.cs`, `Template.UnitTests/Application/EnderecoServiceTests.cs`
  - Cobre: CA-03, CA-04, CA-06, CA-08, CA-09 e CA-10 (unidade)
  - Testes: criar válido chama `AdicionarAsync` + `CommitAsync` e devolve `Ativo = true`; `ExcluirAsync` chama `Remover` + `CommitAsync`;
    `AtivarAsync` deixa `Ativo = true`. **Falha:** request inválido lança `ValidationException` sem commit; usuário inexistente
    no criar e ao trocar o usuário no atualizar lança `NotFoundException` sem commit; id inexistente em obter, ativar, inativar e excluir
    lança `NotFoundException`
  - Verificação: `dotnet test --project backend/tests/Template.UnitTests --filter-class "*EnderecoServiceTests"`
- [x] **T04. `EnderecosController`: `Ativar`, e `Inativar`/`Excluir` com 204 (ADR 0016) + testes funcionais**
  - Arquivos: `Template.Api/Controllers/EnderecosController.cs`, `Template.FunctionalTests/EnderecosTests.cs`
  - Cobre: CA-01, CA-02, CA-03, CA-04, CA-06, CA-07, CA-08, CA-09 e CA-10 (funcional)
  - Testes: listar 25 → 20 itens e total; `POST` → 201 + `Location` com `ativo: true`; `PUT` → 200 com os dados novos (inclui trocar o usuário);
    `PATCH /inativar` e `/ativar` → 204 e o status muda; `DELETE` → 204 e depois `GET` → 404.
    **Falha:** `pageSize=101` e `page=0` → 400; campos vazios → 400 com `errors`; usuário inexistente → 404;
    id inexistente em `GET`, `PUT`, `PATCH` e `DELETE` → 404
  - Verificação: `dotnet test backend` todo verde; os 7 endpoints aparecem e respondem pelo `/scalar`

## Fase 2: Endereco no frontend (skill `angular-crud`)
- [x] **T05. `entities/endereco`** (model com os rótulos do tipo, api com 7 métodos; a status tag foi para `shared/ui` na T05b)
  (a api e o model já tinham sido escritos pelo usuário em `entities/enderecos`; tipos e limites corrigidos para espelhar o backend)
- [x] **T05b. Tag Ativo/Inativo única em `shared/ui`** (descoberta na simplify da T05)
  - `EnderecoStatusTag` e `UsuarioStatusTag` são idênticas: trocar as duas por `shared/ui/ativo-tag` e atualizar
    os usos, o exemplo do ADR 0009 (correção de texto) e a skill `angular-crud`
  - Verificação: `npm test && npm run lint`
  - Arquivos: renomear `entities/enderecos` → `entities/endereco`; `model/endereco.model.ts`, `api/endereco.api.ts`,
    `ui/endereco-status-tag.ts`, `index.ts` e os `.spec.ts`
  - Verificação: `npm test` (URL e verbo de cada método, inclusive `DELETE` em `excluir`)
- [x] **T06. `features/endereco-form`** (criar e editar, seletor de usuário e de tipo)
  - Arquivos: `features/endereco-form/*`, `src/ng-zorro.less` (`select`)
  - Cobre: CA-04 (frontend unit)
  - Testes: form válido chama `criar` e emite `salvo`; o seletor lista os usuários de `UsuarioApi.listar(1, 100)`.
    **Falha:** form vazio não chama a API e mostra os erros; UF e CEP inválidos mostram a mensagem; um 400 do servidor
    exibe o erro no campo; um 404 de usuário mostra a mensagem
  - Verificação: `npm test`
- [ ] **T07. `features/endereco-toggle-ativo` e `features/endereco-excluir`**
  - Arquivos: `features/endereco-toggle-ativo/*`, `features/endereco-excluir/*`
  - Cobre: CA-09 e CA-11 (frontend unit)
  - Testes: confirmar a exclusão chama `excluir` e emite `excluido`; o toggle chama `ativar` ou `inativar`.
    **Falha:** cancelar a confirmação não chama a API
  - Verificação: `npm test`
- [ ] **T08. `widgets/enderecos-table`**
  - Arquivos: `widgets/enderecos-table/*`
  - Testes: trocar de página chama `listar` com `page` e `pageSize` novos; a coluna usuário mostra o nome a partir do `usuarioId`;
    na página N, `irParaPrimeiraPagina()` chama `listar(1, pageSize)`. **Falha:** `usuarioId` fora da lista mostra um texto neutro, sem quebrar
  - Verificação: `npm test`
- [ ] **T09. `pages/enderecos-page` + item de menu** (a rota lazy já existe em `app.routes.ts`)
  - Arquivos: `pages/enderecos-page/*`, `app/app.ts` (`menu`)
  - Verificação: `npm run build && npm run lint` (o build volta a passar); teste manual: cadastrar, editar, inativar e excluir pela tela

## Fase 3: Ponta a ponta e fechamento
- [ ] **T10. e2e de endereços**
  - Arquivos: `frontend/e2e/enderecos.spec.ts`
  - Cobre: CA-03, CA-04 e CA-10 (e2e)
  - Testes: cadastrar endereço e vê-lo na tabela; excluir e ele some. **Falha:** submeter vazio mostra erros e não cria
  - Verificação: `npm run e2e` (com `docker compose up -d` e a API rodando)
- [ ] **T11. Fechamento**
  - code-review, cobertura dos CAs no `plan.md`, study-log, spec `Concluída` no roadmap
  - Verificação: `dotnet test backend && cd frontend && npm run lint && npm test && npm run e2e`
