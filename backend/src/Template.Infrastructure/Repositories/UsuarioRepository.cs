using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;
using Template.Domain.Entities.Interfaces;
using Template.Infrastructure.Data;

namespace Template.Infrastructure.Repositories;

public class UsuarioRepository(AppDbContext context) : RepositoryBase<Usuario>(context), IUsuarioRepository
{
    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken cancellationToken)
    {
        // Normaliza aqui, e não em cada chamador; igualdade simples usa o índice único
        var normalizado = Usuario.NormalizarEmail(email);
        return Set.AnyAsync(x => x.Email == normalizado && x.Id != ignorarId, cancellationToken);
    }
}
