namespace Template.Domain.Entities.Interfaces;

public interface IUsuarioRepository : IRepositoryBase<Usuario>
{
    // Recebe o e-mail como foi digitado; a implementação normaliza (RN-02) e compara por igualdade,
    // para usar o índice único da coluna
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken cancellationToken);
}
