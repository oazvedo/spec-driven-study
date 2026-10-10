# 0010. Estratégia de testes

- **Status:** Aceito (a biblioteca de asserções foi substituída pelo [ADR 0014](0014-awesomeassertions-no-lugar-de-fluentassertions.md))
- **Data:** 2026-10-10

## Contexto
O código é gerado com apoio de IA, e os testes são a forma objetiva de verificar que ele faz o que a spec pede,
inclusive que **rejeita** o que deve rejeitar.

## Decisão
Cinco tipos de teste. **Todo recurso tem, em cada nível, pelo menos um cenário de sucesso e um de falha.**
O cenário de falha verifica que o sistema rejeita a entrada ou o estado inválido: o teste passa
porque o erro esperado aconteceu.

| Nível | Onde / ferramentas | O que cobre | Sucesso (ex.) | Falha (ex.) |
|---|---|---|---|---|
| Unidade | `backend/tests/Template.UnitTests`: xUnit, NSubstitute, FluentAssertions. Frontend: Vitest | Entidades, validators e services com repositórios mockados | Criar usuário válido chama `AdicionarAsync` + `CommitAsync` | E-mail inválido lança `ValidationException` e **não** chama `CommitAsync` |
| Integração | `backend/tests/Template.IntegrationTests`: Testcontainers (Postgres) | Repositórios, `XxxConfig` e migrations contra um banco real | Salvar e listar paginado | E-mail duplicado viola o índice único |
| Funcional | `backend/tests/Template.FunctionalTests`: `WebApplicationFactory` + Testcontainers | Requisição HTTP → resposta, com a API inteira | `POST` → 201 + `Location` | Payload inválido → 400; `pageSize=101` → 400; id inexistente → 404 |
| Ponta a ponta | `frontend/e2e`: Playwright | Fluxos do usuário no navegador, com API e banco reais | Cadastrar usuário e vê-lo na tabela | Form vazio mostra erros e não cria registro |
| Arquitetura | `backend/tests/Template.ArchitectureTests`: NetArchTest | Regras de dependência do ADR 0003 | Domain não referencia EF Core, Application, etc. | (falha se alguém violar) |

Convenções:
- Nome do teste: `Metodo_Cenario_ResultadoEsperado` (ex.: `CriarAsync_EmailInvalido_LancaValidationException`).
- Estrutura AAA (Arrange, Act, Assert).
- Os critérios de aceite da spec viram testes. O `tasks.md` aponta qual teste cobre cada critério.
- Uma task só termina com `dotnet test` / `npm test` verdes.

## Alternativas consideradas
- **Moq:** popular, mas o NSubstitute tem uma sintaxe mais enxuta e não teve a polêmica de telemetria (SponsorLink).
- **Banco em memória do EF Core nos testes:** não reproduz constraints nem SQL reais.
- **Só testes de unidade:** não pegam erros de mapeamento, de migrations ou de HTTP.

## Consequências
- ✅ Cada camada tem uma rede de segurança, e os cenários de falha documentam as regras.
- ⚠️ Testes de integração e funcionais exigem Docker e são mais lentos.
