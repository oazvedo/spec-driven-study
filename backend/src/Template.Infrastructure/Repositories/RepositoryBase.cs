using Microsoft.EntityFrameworkCore;
using Template.Domain.Common;
using Template.Domain.Entities.Interfaces;
using Template.Infrastructure.Data;

namespace Template.Infrastructure.Repositories;

// Só rastreia mudanças; quem persiste é o IUnitOfWork (ADR 0004)
public abstract class RepositoryBase<T>(AppDbContext context) : IRepositoryBase<T> where T : EntityBase
{
    protected DbSet<T> Set { get; } = context.Set<T>();

    // FindAsync reaproveita a entidade se ela já estiver rastreada no contexto
    public Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
        => Set.FindAsync([id], cancellationToken).AsTask();

    // SELECT EXISTS pela PK: não carrega nem rastreia a entidade
    public Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken)
        => Set.AnyAsync(x => x.Id == id, cancellationToken);

    public async Task<PagedResult<T>> ListarPaginadoAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        // Id desempata registros salvos no mesmo commit (mesmo CreatedAt) e mantém a paginação estável.
        // Não reflete a ordem de inserção dentro do commit: Guid v7 só é ordenado entre milissegundos.
        var query = Set.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        var total = await query.CountAsync(cancellationToken);
        // Em long, page grande não estoura o int. Página além do fim volta vazia sem consultar de novo.
        var pular = (long)(page - 1) * pageSize;
        var items = pular >= total
            ? []
            : await query
                .Skip((int)pular)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, total);
    }

    // O Id já vem da entidade (Guid v7), então AddAsync não traria ganho
    public Task AdicionarAsync(T entity, CancellationToken cancellationToken)
    {
        Set.Add(entity);
        return Task.CompletedTask;
    }

    // Entidade rastreada: o change tracker já detecta só o que mudou. Update só para entidade desanexada.
    public void Atualizar(T entity)
    {
        if (context.Entry(entity).State == EntityState.Detached)
            Set.Update(entity);
    }

    public void Remover(T entity) => Set.Remove(entity);
}
