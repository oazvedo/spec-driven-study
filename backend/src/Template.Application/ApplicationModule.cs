using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Services;
using Template.Application.Services.Interfaces;

namespace Template.Application;

// Único ponto de registro da camada Application (ADR 0003)
public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddValidatorsFromAssembly(typeof(ApplicationModule).Assembly);
        return services;
    }
}
