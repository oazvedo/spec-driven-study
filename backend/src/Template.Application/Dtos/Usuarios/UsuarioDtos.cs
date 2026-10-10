using Template.Domain.Entities;

namespace Template.Application.Dtos.Usuarios;

public record CriarUsuarioRequest(string Nome, string Email);

public record AtualizarUsuarioRequest(string Nome, string Email);

public record UsuarioResponse(Guid Id, string Nome, string Email, bool Ativo, DateTime CreatedAt);

// Mapeamento manual (ADR 0007)
public static class UsuarioMappings
{
    public static UsuarioResponse ToResponse(this Usuario u)
        => new(u.Id, u.Nome, u.Email, u.Ativo, u.CreatedAt);
}
