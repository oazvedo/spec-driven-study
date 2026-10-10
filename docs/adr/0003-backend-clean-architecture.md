# 0003. Backend em Clean Architecture

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
O backend precisa de uma estrutura previsível, em que cada tipo de código tenha um lugar óbvio,
para que a IA e as pessoas sigam o mesmo padrão em todas as entidades.

## Decisão
Quatro projetos em `backend/src/`:

| Projeto | Contém | Referencia |
|---|---|---|
| `Template.Domain` | `Common/` (`EntityBase`, `PagedResult`, `Exceptions/`) e `Entities/` (entidades + `Interfaces/` com `IRepositoryBase<T>`, `IUnitOfWork` e `IXxxRepository`) | nada |
| `Template.Application` | `Dtos/`, `Validators/`, `Services/` (+ `Services/Interfaces/`) e `ApplicationModule.cs` | Domain |
| `Template.Infrastructure` | `Data/` (`AppDbContext`, `UnitOfWork`, `Configurations/`, `Migrations/`), `Repositories/` e `InfraModule.cs` | Domain |
| `Template.Api` | `Controllers/`, `ErrorHandling/` e `Program.cs` | Application, Infrastructure |

Convenções de pasta e nome (exemplo com `Usuario`):
```
Domain/Entities/Usuario.cs
Domain/Entities/Interfaces/IUsuarioRepository.cs        // : IRepositoryBase<Usuario>
Application/Services/UsuarioService.cs
Application/Services/Interfaces/IUsuarioService.cs
Infrastructure/Data/Configurations/UsuarioConfig.cs
Infrastructure/Repositories/UsuarioRepository.cs
Api/Controllers/UsuariosController.cs
```

**Registro de DI:** cada camada expõe um módulo, e o `Program.cs` só chama os módulos:
```csharp
// Application/ApplicationModule.cs
public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddValidatorsFromAssembly(typeof(ApplicationModule).Assembly);
        return services;
    }
}

// Infrastructure/InfraModule.cs
public static class InfraModule
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        return services;
    }
}

// Api/Program.cs
builder.Services.AddApplication().AddInfra(builder.Configuration);
```

**Nomenclatura:** termos de domínio e casos de uso em português (`Usuario`, `ListarAsync`,
`CriarUsuarioRequest`); termos técnicos em inglês (`Repository`, `Service`, `Config`, `UnitOfWork`).

As regras de dependência são verificadas por testes de arquitetura (ADR 0010).

## Alternativas consideradas
- **Vertical slice (pasta por feature):** menos arquivos, mas o objetivo aqui é estudar Clean Architecture.
- **Projeto único com pastas:** nenhuma regra de dependência seria garantida pelo compilador.
- **MediatR/CQRS:** mais uma camada de indireção, sem necessidade para um CRUD.

## Consequências
- ✅ Cada arquivo tem um lugar óbvio, e a skill `dotnet-crud` vira uma receita mecânica.
- ✅ O Domain não depende de EF Core, o que facilita testes de unidade.
- ⚠️ Mais arquivos por entidade (cerca de 10). É aceitável em troca da previsibilidade.
