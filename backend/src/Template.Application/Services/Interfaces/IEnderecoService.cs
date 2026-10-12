using Template.Application.Dtos.Enderecos;
using Template.Domain.Common;

namespace Template.Application.Services.Interfaces;

public interface IEnderecoService
{
    Task<PagedResult<EnderecoResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<EnderecoResponse> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<EnderecoResponse> CriarAsync(CriarEnderecoRequest request, CancellationToken cancellationToken);
    Task<EnderecoResponse> AtualizarAsync(Guid id, AtualizarEnderecoRequest request, CancellationToken cancellationToken);
}
