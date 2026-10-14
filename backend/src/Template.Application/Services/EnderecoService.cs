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
    // Uma consulta de nomes para a página inteira; a FK garante que todo usuarioId existe
    public async Task<PagedResult<EnderecoResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var pagina = await enderecoRepository.ListarPaginadoAsync(page, pageSize, cancellationToken);
        var nomes = await usuarioRepository.ObterNomesAsync(pagina.Items.Select(e => e.UsuarioId).Distinct().ToList(), cancellationToken);
        return pagina.Map(e => e.ToResponse(nomes[e.UsuarioId]));
    }

    public async Task<EnderecoResponse> ObterAsync(Guid id, CancellationToken cancellationToken)
    {
        var endereco = await ObterEntidadeAsync(id, cancellationToken);
        return endereco.ToResponse(await ObterNomeDoUsuarioAsync(endereco.UsuarioId, cancellationToken));
    }

    public async Task<EnderecoResponse> CriarAsync(CriarEnderecoRequest request, CancellationToken cancellationToken)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var usuarioNome = await ObterNomeDoUsuarioAsync(request.UsuarioId, cancellationToken);

        var endereco = new Endereco(request.UsuarioId, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Estado, request.Cep, request.Tipo);
        await enderecoRepository.AdicionarAsync(endereco, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return endereco.ToResponse(usuarioNome);
    }

    public async Task<EnderecoResponse> AtualizarAsync(Guid id, AtualizarEnderecoRequest request, CancellationToken cancellationToken)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var endereco = await ObterEntidadeAsync(id, cancellationToken);
        var usuarioNome = await ObterNomeDoUsuarioAsync(request.UsuarioId, cancellationToken);

        endereco.Atualizar(request.UsuarioId, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.Cidade, request.Estado, request.Cep, request.Tipo);
        enderecoRepository.Atualizar(endereco);
        await unitOfWork.CommitAsync(cancellationToken);
        return endereco.ToResponse(usuarioNome);
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

    // Remoção física: endereço não tem valor histórico (ADRs 0015 e 0016)
    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken)
    {
        var endereco = await ObterEntidadeAsync(id, cancellationToken);
        enderecoRepository.Remover(endereco);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<Endereco> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
        => await enderecoRepository.ObterPorIdAsync(id, cancellationToken)
           ?? throw new NotFoundException($"Endereço {id} não encontrado.");

    // O nome vai na resposta e também prova que o usuário existe:
    // 404 com mensagem clara, em vez de deixar a FK falhar no commit
    private async Task<string> ObterNomeDoUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken)
        => (await usuarioRepository.ObterNomesAsync([usuarioId], cancellationToken)).GetValueOrDefault(usuarioId)
           ?? throw new NotFoundException($"Usuário {usuarioId} não encontrado.");
}
