using Microsoft.Extensions.DependencyInjection;

namespace Template.Application;

// Único ponto de registro da camada Application (ADR 0003). Services e validators entram aqui.
public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services;
}
