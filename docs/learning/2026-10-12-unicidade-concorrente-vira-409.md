# Unicidade concorrente: o índice único é a garantia, a checagem do service é só a mensagem
- **Data:** 2026-10-12 · **Spec/Task:** 001 / T10b · **Tags:** dotnet, ef-core, postgres, concorrência

## O que é
O `UsuarioService` pergunta `ExisteEmailAsync` antes de salvar, mas entre a pergunta e o `SaveChanges`
outra requisição pode gravar o mesmo e-mail (race condition do tipo *check-then-act*). Quem garante a regra
de verdade é o índice único do banco. A checagem do service serve para dar uma mensagem amigável
no caso comum.

## Por que importa aqui
Sem tratamento, a segunda requisição estoura `DbUpdateException` e vira 500, o que quebra o CA-06 (esperado: 409).
A tradução fica no `UnitOfWork` (Infrastructure), então vale para todas as entidades e a Application
continua sem conhecer EF Core nem Npgsql (ADR 0003). O `GlobalExceptionHandler` já sabe transformar
`ConflictException` em 409 ProblemDetails (ADR 0006).

## Onde aparece no código
- `backend/src/Template.Infrastructure/Data/UnitOfWork.cs:18`: o filtro `when` por SqlState `23505`.
- `backend/tests/Template.IntegrationTests/Repositories/UsuarioRepositoryTests.cs:141`: commit com e-mail
  duplicado sem passar pelo service → `ConflictException`.

## Exemplo mínimo
```csharp
catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
{
    throw new ConflictException("Já existe um registro com estes dados.", ex);
}
```

## Dúvidas em aberto / para aprofundar
- A mensagem do 409 vindo do índice é genérica. Dá para usar `PostgresException.ConstraintName` para dizer qual campo?
- Exception filters em C# (`catch ... when`): https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements
