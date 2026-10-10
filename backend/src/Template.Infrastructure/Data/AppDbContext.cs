using Microsoft.EntityFrameworkCore;
using Template.Domain.Common;

namespace Template.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as XxxConfig deste assembly (ADR 0005)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Índice da ordenação usada por RepositoryBase.ListarPaginadoAsync, em toda entidade
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(EntityBase).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(EntityBase.CreatedAt), nameof(EntityBase.Id))
                .IsDescending(true, true);
        }
    }

    // Todas as sobrecargas de SaveChanges passam por estas duas
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PreencherDatas();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PreencherDatas();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PreencherDatas()
    {
        var agora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = agora;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.UpdatedAt).CurrentValue = agora;
                entry.Property(x => x.CreatedAt).IsModified = false; // CreatedAt nunca é regravado
            }
        }
    }
}
