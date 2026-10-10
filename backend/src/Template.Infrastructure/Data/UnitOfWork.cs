using Template.Domain.Entities.Interfaces;

namespace Template.Infrastructure.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken cancellationToken)
        => context.SaveChangesAsync(cancellationToken);
}
