using AwesomeAssertions;
using FluentValidation;
using NSubstitute;
using Template.Application.Dtos.Enderecos;
using Template.Application.Services;
using Template.Application.Validators;
using Template.Domain.Common;
using Template.Domain.Common.Exceptions;
using Template.Domain.Entities;
using Template.Domain.Entities.Enums;
using Template.Domain.Entities.Interfaces;

namespace Template.UnitTests.Application.Services;

// Repositórios e Unit of Work mockados; validators reais (são regras de negócio, não infraestrutura)
public class EnderecoServiceTests
{
    private readonly IEnderecoRepository _repository = Substitute.For<IEnderecoRepository>();
    private readonly IUsuarioRepository _usuarioRepository = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly EnderecoService _sut;

    private static readonly Guid UsuarioId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public EnderecoServiceTests()
    {
        _sut = new EnderecoService(_repository, _usuarioRepository, _unitOfWork,
            new CriarEnderecoRequestValidator(), new AtualizarEnderecoRequestValidator());
        _usuarioRepository.ExisteAsync(UsuarioId, Arg.Any<CancellationToken>()).Returns(true);
    }

    private static CriarEnderecoRequest CriarRequest(Guid usuarioId)
        => new(usuarioId, "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Comercial);

    private static AtualizarEnderecoRequest AtualizarRequest(Guid usuarioId)
        => new(usuarioId, "Rua Augusta", "500", "Apto 12", "Consolação", "São Paulo", "SP", "01305000", EnderecoTipo.Residencial);

    private Endereco EnderecoExistente()
    {
        var endereco = new Endereco(UsuarioId, "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Comercial);
        _repository.ObterPorIdAsync(endereco.Id, Arg.Any<CancellationToken>()).Returns(endereco);
        return endereco;
    }

    // CA-03
    [Fact]
    public async Task CriarAsync_RequestValido_AdicionaECommita()
    {
        var response = await _sut.CriarAsync(CriarRequest(UsuarioId), Ct);

        response.UsuarioId.Should().Be(UsuarioId);
        response.Ativo.Should().BeTrue();
        await _repository.Received(1).AdicionarAsync(Arg.Is<Endereco>(e => e.Id == response.Id), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-04 (falha)
    [Fact]
    public async Task CriarAsync_RequestInvalido_LancaValidationExceptionSemCommit()
    {
        var act = () => _sut.CriarAsync(CriarRequest(UsuarioId) with { Logradouro = "" }, Ct);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().AdicionarAsync(Arg.Any<Endereco>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-06 (falha)
    [Fact]
    public async Task CriarAsync_UsuarioInexistente_LancaNotFoundSemCommit()
    {
        var act = () => _sut.CriarAsync(CriarRequest(Guid.NewGuid()), Ct);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Usuário * não encontrado.");
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-07
    [Fact]
    public async Task AtualizarAsync_MesmoUsuario_AlteraECommitaSemConsultarUsuario()
    {
        var endereco = EnderecoExistente();

        var response = await _sut.AtualizarAsync(endereco.Id, AtualizarRequest(UsuarioId), Ct);

        response.Logradouro.Should().Be("Rua Augusta");
        response.Tipo.Should().Be(EnderecoTipo.Residencial);
        await _usuarioRepository.DidNotReceive().ExisteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-07: trocar o dono do endereço
    [Fact]
    public async Task AtualizarAsync_OutroUsuarioExistente_TrocaOUsuario()
    {
        var endereco = EnderecoExistente();
        var outroUsuarioId = Guid.NewGuid();
        _usuarioRepository.ExisteAsync(outroUsuarioId, Arg.Any<CancellationToken>()).Returns(true);

        var response = await _sut.AtualizarAsync(endereco.Id, AtualizarRequest(outroUsuarioId), Ct);

        response.UsuarioId.Should().Be(outroUsuarioId);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-06 (falha)
    [Fact]
    public async Task AtualizarAsync_TrocaParaUsuarioInexistente_LancaNotFoundSemAlterar()
    {
        var endereco = EnderecoExistente();

        var act = () => _sut.AtualizarAsync(endereco.Id, AtualizarRequest(Guid.NewGuid()), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
        endereco.UsuarioId.Should().Be(UsuarioId);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-08 (falha)
    [Fact]
    public async Task ObterAsync_IdInexistente_LancaNotFoundException()
    {
        var act = () => _sut.ObterAsync(Guid.NewGuid(), Ct);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Endereço * não encontrado.");
    }

    // CA-08 (falha)
    [Fact]
    public async Task AtivarInativarExcluir_IdInexistente_LancaNotFoundSemCommit()
    {
        var id = Guid.NewGuid();

        await FluentActions.Awaiting(() => _sut.AtivarAsync(id, Ct)).Should().ThrowAsync<NotFoundException>();
        await FluentActions.Awaiting(() => _sut.InativarAsync(id, Ct)).Should().ThrowAsync<NotFoundException>();
        await FluentActions.Awaiting(() => _sut.ExcluirAsync(id, Ct)).Should().ThrowAsync<NotFoundException>();
        _repository.DidNotReceive().Remover(Arg.Any<Endereco>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-09
    [Fact]
    public async Task InativarEAtivar_EnderecoExistente_AlteraStatusECommita()
    {
        var endereco = EnderecoExistente();

        await _sut.InativarAsync(endereco.Id, Ct);
        endereco.Ativo.Should().BeFalse();

        await _sut.AtivarAsync(endereco.Id, Ct);
        endereco.Ativo.Should().BeTrue();

        await _unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-10
    [Fact]
    public async Task ExcluirAsync_EnderecoExistente_RemoveECommita()
    {
        var endereco = EnderecoExistente();

        await _sut.ExcluirAsync(endereco.Id, Ct);

        _repository.Received(1).Remover(endereco);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-01
    [Fact]
    public async Task ListarAsync_PaginaComItens_RetornaResponsesComPaginacao()
    {
        var endereco = EnderecoExistente();
        _repository.ListarPaginadoAsync(2, 10, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Endereco>([endereco], 2, 10, 11));

        var pagina = await _sut.ListarAsync(2, 10, Ct);

        pagina.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new { endereco.Id, Ativo = true });
        pagina.Should().BeEquivalentTo(new { Page = 2, PageSize = 10, TotalCount = 11 });
    }
}
