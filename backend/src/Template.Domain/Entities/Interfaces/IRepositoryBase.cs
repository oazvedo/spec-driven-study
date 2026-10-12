using Template.Domain.Common;

namespace Template.Domain.Entities.Interfaces;

// Operações comuns a todas as entidades. Não persiste: quem confirma é o IUnitOfWork (ADR 0004).
public interface IRepositoryBase<T> where T : EntityBase
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<T>> ListarPaginadoAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task AdicionarAsync(T entity, CancellationToken cancellationToken);
    void Atualizar(T entity);
    void Remover(T entity); // remoção física (ADR 0015)
}
