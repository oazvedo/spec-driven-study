# 0006. Validação com FluentValidation e erros como ProblemDetails

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
As regras de entrada precisam ficar testáveis e fora dos controllers, e todos os erros da API
precisam chegar ao frontend em um formato único.

## Decisão
- **FluentValidation** na Application, em `Application/Validators/` (um validator por request:
  `CriarUsuarioRequestValidator`, `AtualizarUsuarioRequestValidator`). Os validators são registrados
  por `AddValidatorsFromAssembly` no `ApplicationModule`.
- **O service valida explicitamente** (`await validator.ValidateAndThrowAsync(request, ct)`).
  Não usamos auto-validation do pipeline do ASP.NET, para que a regra fique visível e possa ser testada na unidade.
- As exceções de negócio ficam em `Domain/Common/Exceptions/`: `NotFoundException` e `ConflictException`.
- Um `GlobalExceptionHandler` (`IExceptionHandler`) em `Api/ErrorHandling/` converte as exceções
  em **ProblemDetails** (RFC 9457):

| Exceção | Status | Corpo |
|---|---|---|
| `FluentValidation.ValidationException` | 400 | `ValidationProblemDetails` com `errors` por campo |
| `NotFoundException` | 404 | `ProblemDetails` |
| `ConflictException` | 409 | `ProblemDetails` (ex.: e-mail já cadastrado) |
| qualquer outra | 500 | `ProblemDetails` genérico, sem stack trace |

- Os parâmetros de rota e de query (`[Range]` em `page` e `pageSize`) são validados pelo `[ApiController]`,
  que já responde 400 com `ValidationProblemDetails`.
- As mensagens de validação são escritas em português.

## Alternativas consideradas
- **Data annotations nos DTOs:** espalham as regras e são difíceis de compor e testar.
- **Result pattern em vez de exceções:** mais explícito, mas exige mais código em todo service e controller.
  Para um CRUD de estudos, as exceções somadas a um handler central são mais simples.

## Consequências
- ✅ Os controllers ficam finos: não têm `try/catch` nem `if (!valid)`.
- ✅ O frontend trata um único formato de erro.
- ⚠️ Exceções como controle de fluxo têm um custo de desempenho, irrelevante nesta escala.
