using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure.Data;

// Todas as classes do projeto dividem o mesmo banco e o limpam antes de cada teste:
// em paralelo, uma apagaria os dados da outra
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace Template.Tests.Shared;

// Compartilhado (via <Compile Link>) pelos projetos de teste que sobem Postgres real
internal static class BancoDeTeste
{
    public const string ImagemPostgres = "postgres:18-alpine";

    // Esvazia todas as tabelas do modelo, para cada teste começar com o banco limpo
    public static async Task LimparBancoAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tabelas = db.Model.GetEntityTypes()
            .Select(t => $"\"{t.GetTableName()}\"")
            .Distinct();
        var sql = $"TRUNCATE TABLE {string.Join(", ", tabelas)} CASCADE";

        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
