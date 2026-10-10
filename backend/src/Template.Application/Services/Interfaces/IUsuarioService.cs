using Template.Application.Dtos.Usuarios;
using Template.Domain.Common;

namespace Template.Application.Services.Interfaces;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<UsuarioResponse> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken);
    Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request, CancellationToken cancellationToken);
    Task AtivarAsync(Guid id, CancellationToken cancellationToken);
    Task InativarAsync(Guid id, CancellationToken cancellationToken);
}
