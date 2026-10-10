using Microsoft.EntityFrameworkCore;
using Npgsql;
using Template.Domain.Common.Exceptions;
using Template.Domain.Entities.Interfaces;

namespace Template.Infrastructure.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        // Rede de segurança dos índices únicos: duas requisições simultâneas passam pela checagem do service
        // e a segunda só falha aqui. Vira 409 (ConflictException) em vez de 500, para qualquer entidade.
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException("Já existe um registro com estes dados.", ex);
        }
    }
}
