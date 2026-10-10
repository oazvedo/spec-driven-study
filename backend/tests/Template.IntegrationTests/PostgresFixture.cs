using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure;
using Template.Infrastructure.Data;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Template.IntegrationTests.PostgresFixture))]

namespace Template.IntegrationTests;

// Um Postgres real (Testcontainers) para todos os testes de integração, registrado pelo próprio InfraModule
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public ServiceProvider Services { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _postgres.GetConnectionString()
            })
            .Build();

        Services = new ServiceCollection().AddInfra(configuration).BuildServiceProvider();
        await Services.ApplyMigrationsAsync();
    }

    // Esvazia todas as tabelas do modelo, para cada teste começar com o banco limpo
    public async Task LimparBancoAsync(CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tabelas = db.Model.GetEntityTypes()
            .Select(t => $"\"{t.GetTableName()}\"")
            .Distinct();
        var sql = $"TRUNCATE TABLE {string.Join(", ", tabelas)} CASCADE";

        await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
