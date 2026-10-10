using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Infrastructure;
using Template.Infrastructure.Data;
using Template.Tests.Shared;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Template.IntegrationTests.PostgresFixture))]

namespace Template.IntegrationTests;

// Um Postgres real (Testcontainers) para todos os testes de integração, registrado pelo próprio InfraModule
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(BancoDeTeste.ImagemPostgres).Build();

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

    public Task LimparBancoAsync(CancellationToken cancellationToken) => Services.LimparBancoAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
