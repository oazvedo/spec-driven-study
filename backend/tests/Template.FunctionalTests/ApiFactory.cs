using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Template.Tests.Shared;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Template.FunctionalTests.ApiFactory))]

namespace Template.FunctionalTests;

// Sobe um Postgres real (Testcontainers) uma vez para todos os testes funcionais.
// A própria API aplica as migrations ao iniciar (ADR 0013).
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(BancoDeTeste.ImagemPostgres).Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        StartServer(); // sobe a API (e aplica as migrations) uma vez, antes dos testes rodarem em paralelo
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());

    public Task LimparBancoAsync(CancellationToken cancellationToken) => Services.LimparBancoAsync(cancellationToken);

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
