using AwesomeAssertions;
using Template.Domain.Common;

namespace Template.UnitTests.Domain.Common;

public class EntityBaseTests
{
    private sealed class EntidadeFake : EntityBase;

    [Fact]
    public void NovaEntidade_NasceAtivaComIdPreenchido()
    {
        var entidade = new EntidadeFake();

        entidade.Ativo.Should().BeTrue();
        entidade.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Inativar_DefineAtivoFalse()
    {
        var entidade = new EntidadeFake();

        entidade.Inativar();

        entidade.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Ativar_EntidadeInativa_DefineAtivoTrue()
    {
        var entidade = new EntidadeFake();
        entidade.Inativar();

        entidade.Ativar();

        entidade.Ativo.Should().BeTrue();
    }
}
