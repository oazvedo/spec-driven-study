using FluentValidation;
using Template.Application.Dtos.Usuarios;
using Template.Application.Services.Interfaces;
using Template.Domain.Common;
using Template.Domain.Common.Exceptions;
using Template.Domain.Entities;
using Template.Domain.Entities.Interfaces;

namespace Template.Application.Services;

// Fluxo de escrita: validar → carregar → regra de negócio → alterar a entidade → um CommitAsync (ADR 0004)
public class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IUnitOfWork unitOfWork,
    IValidator<CriarUsuarioRequest> criarValidator,
    IValidator<AtualizarUsuarioRequest> atualizarValidator) : IUsuarioService
{
    public async Task<PagedResult<UsuarioResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken)
        => (await usuarioRepository.ListarPaginadoAsync(page, pageSize, cancellationToken)).Map(u => u.ToResponse());

    public async Task<UsuarioResponse> ObterAsync(Guid id, CancellationToken cancellationToken)
        => (await ObterEntidadeAsync(id, cancellationToken)).ToResponse();

    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirEmailUnicoAsync(request.Email, null, cancellationToken);

        var usuario = new Usuario(request.Nome, request.Email);
        await usuarioRepository.AdicionarAsync(usuario, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return usuario.ToResponse();
    }

    public async Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var usuario = await ObterEntidadeAsync(id, cancellationToken);
        if (Usuario.NormalizarEmail(request.Email) != usuario.Email) // e-mail igual não precisa consultar unicidade
            await GarantirEmailUnicoAsync(request.Email, id, cancellationToken);

        usuario.Atualizar(request.Nome, request.Email);
        usuarioRepository.Atualizar(usuario);
        await unitOfWork.CommitAsync(cancellationToken);
        return usuario.ToResponse();
    }

    public async Task AtivarAsync(Guid id, CancellationToken cancellationToken)
    {
        (await ObterEntidadeAsync(id, cancellationToken)).Ativar();
        await unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task InativarAsync(Guid id, CancellationToken cancellationToken)
    {
        (await ObterEntidadeAsync(id, cancellationToken)).Inativar();
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<Usuario> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
        => await usuarioRepository.ObterPorIdAsync(id, cancellationToken)
           ?? throw new NotFoundException($"Usuário {id} não encontrado.");

    private async Task GarantirEmailUnicoAsync(string email, Guid? ignorarId, CancellationToken cancellationToken)
    {
        if (await usuarioRepository.ExisteEmailAsync(email, ignorarId, cancellationToken))
            throw new ConflictException("Já existe um usuário com este e-mail.");
    }
}
