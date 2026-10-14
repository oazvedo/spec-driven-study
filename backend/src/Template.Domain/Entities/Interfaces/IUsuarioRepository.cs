namespace Template.Domain.Entities.Interfaces;

public interface IUsuarioRepository : IRepositoryBase<Usuario>
{
    // Recebe o e-mail como foi digitado; a implementação normaliza (RN-02) e compara por igualdade,
    // para usar o índice único da coluna
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken cancellationToken);

    // Nome de cada usuário pedido, em uma consulta só; ids inexistentes ficam fora do dicionário
    Task<IReadOnlyDictionary<Guid, string>> ObterNomesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}
