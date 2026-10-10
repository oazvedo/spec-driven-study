namespace Template.Domain.Entities.Interfaces;

// Chamado uma vez por caso de uso, pelo service (ADR 0004)
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken);
}
