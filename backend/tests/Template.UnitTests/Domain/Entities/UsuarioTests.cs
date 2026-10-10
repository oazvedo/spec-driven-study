using AwesomeAssertions;
using Template.Domain.Entities;

namespace Template.UnitTests.Domain.Entities;

public class UsuarioTests
{
    [Fact]
    public void Construtor_DadosValidos_GuardaOsDados()
    {
        var usuario = new Usuario("Ana", "ana@x.com");

        usuario.Nome.Should().Be("Ana");
        usuario.Email.Should().Be("ana@x.com");
    }

    // RN-02 / CA-06: também remove espaços nas pontas
    [Fact]
    public void Usuario_EmailComMaiusculas_GuardaEmMinusculas()
    {
        var usuario = new Usuario("Ana", "  Ana@X.COM ");

        usuario.Email.Should().Be("ana@x.com");
    }

    // CA-07
    [Fact]
    public void Atualizar_TrocaNomeEEmailNormalizado()
    {
        var usuario = new Usuario("Ana", "ana@x.com");

        usuario.Atualizar("Ana Maria", "AnaMaria@X.com");

        usuario.Nome.Should().Be("Ana Maria");
        usuario.Email.Should().Be("anamaria@x.com");
    }

    // CA-09
    [Fact]
    public void Usuario_Inativar_DefineAtivoFalse()
    {
        var usuario = new Usuario("Ana", "ana@x.com");

        usuario.Inativar();

        usuario.Ativo.Should().BeFalse();
    }
}
