using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Domain.Entities.Interfaces;
using Template.Infrastructure.Data;
using Template.Infrastructure.Repositories;

namespace Template.Infrastructure;

// Único ponto de registro da camada Infrastructure (ADR 0003)
public static class InfraModule
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("Default"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        return services;
    }
}
