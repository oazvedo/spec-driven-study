using FluentValidation;
using Template.Application.Dtos.Enderecos;
using Template.Application.Services.Interfaces;
using Template.Domain.Common;
using Template.Domain.Common.Exceptions;
using Template.Domain.Entities;
using Template.Domain.Entities.Interfaces;

namespace Template.Application.Services;

// Fluxo de escrita: validar → carregar → regra de negócio → alterar a entidade → um CommitAsync (ADR 0004)
public class EnderecoService(
    IEnderecoRepository enderecoRepository,
    IUsuarioRepository usuarioRepository,
    IUnitOfWork unitOfWork,
    IValidator<CriarEnderecoRequest> criarValidator,
    IValidator<AtualizarEnderecoRequest> atualizarValidator) : IEnderecoService
{
    public async Task<PagedResult<EnderecoResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken)
        => (await enderecoRepository.ListarPaginadoAsync(page, pageSize, cancellationToken)).Map(e => e.ToResponse());

    public async Task<EnderecoResponse> ObterAsync(Guid id, CancellationToken cancellationToken)
        => (await ObterEntidadeAsync(id, cancellationToken)).ToResponse();

    public async Task<EnderecoResponse> CriarAsync(CriarEnderecoRequest request, CancellationToken cancellationToken)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirUsuarioExisteAsync(request.UsuarioId, cancellationToken);

        var endereco = new Endereco(request.UsuarioId, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Estado, request.Cep, request.Tipo);
        await enderecoRepository.AdicionarAsync(endereco, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return endereco.ToResponse();
    }

    public async Task<EnderecoResponse> AtualizarAsync(Guid id, AtualizarEnderecoRequest request, CancellationToken cancellationToken)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var endereco = await ObterEntidadeAsync(id, cancellationToken);
        if (request.UsuarioId != endereco.UsuarioId) // mesmo usuário não precisa consultar
            await GarantirUsuarioExisteAsync(request.UsuarioId, cancellationToken);

        endereco.Atualizar(request.UsuarioId, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Estado, request.Cep, request.Tipo);
        enderecoRepository.Atualizar(endereco);
        await unitOfWork.CommitAsync(cancellationToken);
        return endereco.ToResponse();
    }

    private async Task<Endereco> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
        => await enderecoRepository.ObterPorIdAsync(id, cancellationToken)
           ?? throw new NotFoundException($"Endereço {id} não encontrado.");

    // 404 com mensagem clara, em vez de deixar a FK falhar no commit
    private async Task GarantirUsuarioExisteAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        if (!await usuarioRepository.ExisteAsync(usuarioId, cancellationToken))
            throw new NotFoundException($"Usuário {usuarioId} não encontrado.");
    }
}
