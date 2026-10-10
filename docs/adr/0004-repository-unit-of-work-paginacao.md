# 0004. Repository, Unit of Work e PagedResult

- **Status:** Aceito (a regra "não há remoção física" foi substituída pelo [ADR 0015](0015-remocao-fisica-no-repositorio.md))
- **Data:** 2026-10-10

## Contexto
Os services precisam de operações de dados comuns a todas as entidades, de uma forma única de confirmar
uma transação e de um formato padronizado de resposta paginada.

## Decisão

**`IRepositoryBase<T>`** (Domain) com as operações básicas, implementado por `RepositoryBase<T>` (Infrastructure):
```csharp
public interface IRepositoryBase<T> where T : EntityBase
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<T>> ListarPaginadoAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task AdicionarAsync(T entity, CancellationToken cancellationToken);
    void Atualizar(T entity);
}
```
- Cada entidade tem uma interface própria que herda da base: `IUsuarioRepository : IRepositoryBase<Usuario>`.
  Consultas específicas (por exemplo `ExisteEmailAsync`) entram só na interface da entidade.
- **Os repositórios nunca chamam `SaveChanges`.** Eles só rastreiam as mudanças.
- Não há remoção física: a exclusão é lógica (`Ativar()` e `Inativar()`, ADR 0008).

**`IUnitOfWork`** (Domain) confirma a transação. O service chama `CommitAsync` uma vez por caso de uso:
```csharp
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}
```

**`PagedResult<T>`** (Domain/Common) é o formato único de toda listagem:
```csharp
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> map)
        => new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
```
O repositório devolve `PagedResult<Entidade>`, e o service converte com `.Map(x => x.ToResponse())`.

**`EntityBase`** (Domain/Common): `Id` (Guid v7), `CreatedAt`, `UpdatedAt` e `Ativo` (padrão `true`),
com os métodos `Ativar()` e `Inativar()` (exclusão lógica, ADR 0008). Todas as propriedades têm setter privado.
As datas são preenchidas no `AppDbContext.SaveChangesAsync` via `ChangeTracker`.

## Alternativas consideradas
- **`DbContext` direto no service:** menos código, mas acopla a Application ao EF Core e foge da regra do ADR 0003.
- **Repositório genérico sem interfaces por entidade:** não comporta consultas específicas de forma tipada.
- **`SaveChanges` dentro do repositório:** impede juntar várias operações em uma transação.

## Consequências
- ✅ O service controla a transação, e cada caso de uso tem exatamente um commit.
- ✅ Todo endpoint de listagem responde no mesmo formato, e o frontend tem um único tipo `PagedResult<T>`.
- ⚠️ O EF Core já é um Unit of Work. Esta camada é uma abstração a mais, aceita para manter o Domain e a Application livres de EF.
