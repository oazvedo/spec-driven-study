using AwesomeAssertions;
using Template.Application.Dtos.Usuarios;
using Template.Domain.Entities;

namespace Template.UnitTests.Application.Dtos;

public class UsuarioMappingsTests
{
    [Fact]
    public void ToResponse_CopiaOsCamposDaEntidade()
    {
        var usuario = new Usuario("Ana", "ana@x.com");
        usuario.Inativar();

        var response = usuario.ToResponse();

        response.Should().BeEquivalentTo(new { usuario.Id, Nome = "Ana", Email = "ana@x.com", Ativo = false, usuario.CreatedAt });
    }
}
