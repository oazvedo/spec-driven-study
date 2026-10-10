using System.Reflection;
using NetArchTest.Rules;

namespace Template.ArchitectureTests;

// Regras de dependência do ADR 0003. Referências entre projetos o compilador já barra;
// estes testes pegam o que ele não barra: pacotes de framework na camada errada.
public class DependenciasEntreCamadasTests
{
    private const string Domain = "Template.Domain";
    private const string Application = "Template.Application";
    private const string Infrastructure = "Template.Infrastructure";
    private const string Api = "Template.Api";

    private const string EfCore = "Microsoft.EntityFrameworkCore";
    private const string AspNetCore = "Microsoft.AspNetCore";
    private const string FluentValidation = "FluentValidation";

    [Fact]
    public void Domain_NaoDependeDeOutrasCamadasNemDeFrameworks()
        => AssertSemDependencias(Domain, Application, Infrastructure, Api, EfCore, AspNetCore, FluentValidation);

    [Fact]
    public void Application_NaoDependeDeInfrastructureApiNemEfCore()
        => AssertSemDependencias(Application, Infrastructure, Api, EfCore, AspNetCore);

    [Fact]
    public void Infrastructure_NaoDependeDeApplicationNemApi()
        => AssertSemDependencias(Infrastructure, Application, Api, AspNetCore);

    private static void AssertSemDependencias(string camada, params string[] proibidas)
    {
        var resultado = Types.InAssembly(Assembly.Load(camada))
            .ShouldNot()
            .HaveDependencyOnAny(proibidas)
            .GetResult();

        var violacoes = resultado.FailingTypeNames ?? [];
        Assert.True(resultado.IsSuccessful,
            $"{camada} depende de [{string.Join(", ", proibidas)}] nos tipos: {string.Join(", ", violacoes)}");
    }
}
