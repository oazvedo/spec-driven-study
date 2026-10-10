using AwesomeAssertions;
using FluentValidation;
using NSubstitute;
using Template.Application.Dtos.Usuarios;
using Template.Application.Services;
using Template.Application.Validators;
using Template.Domain.Common;
using Template.Domain.Common.Exceptions;
using Template.Domain.Entities;
using Template.Domain.Entities.Interfaces;

namespace Template.UnitTests.Application.Services;

// Repositório e Unit of Work mockados; validators reais (são regras de negócio, não infraestrutura)
public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _repository = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UsuarioService _sut;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public UsuarioServiceTests()
        => _sut = new UsuarioService(_repository, _unitOfWork,
            new CriarUsuarioRequestValidator(), new AtualizarUsuarioRequestValidator());

    private Usuario UsuarioExistente(string email = "ana@x.com")
    {
        var usuario = new Usuario("Ana", email);
        _repository.ObterPorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);
        return usuario;
    }

    private void EmailJaExiste() =>
        _repository.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

    // CA-03
    [Fact]
    public async Task CriarAsync_RequestValido_AdicionaECommita()
    {
        var response = await _sut.CriarAsync(new CriarUsuarioRequest("Ana", "Ana@X.com"), Ct);

        response.Email.Should().Be("ana@x.com");
        response.Ativo.Should().BeTrue();
        await _repository.Received(1).AdicionarAsync(Arg.Is<Usuario>(u => u.Id == response.Id), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-04 (falha)
    [Fact]
    public async Task CriarAsync_NomeVazio_LancaValidationExceptionSemCommit()
    {
        var act = () => _sut.CriarAsync(new CriarUsuarioRequest("", "ana@x.com"), Ct);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().AdicionarAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-06 (falha)
    [Fact]
    public async Task CriarAsync_EmailDuplicado_LancaConflictExceptionSemCommit()
    {
        EmailJaExiste();

        var act = () => _sut.CriarAsync(new CriarUsuarioRequest("Ana", "ana@x.com"), Ct);

        await act.Should().ThrowAsync<ConflictException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-07
    [Fact]
    public async Task AtualizarAsync_RequestValido_AlteraECommita()
    {
        var usuario = UsuarioExistente();

        var response = await _sut.AtualizarAsync(usuario.Id, new AtualizarUsuarioRequest("Ana Maria", "anamaria@x.com"), Ct);

        response.Nome.Should().Be("Ana Maria");
        response.Email.Should().Be("anamaria@x.com");
        await _repository.Received(1).ExisteEmailAsync("anamaria@x.com", usuario.Id, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_MesmoEmailComOutraCaixa_NaoConsultaUnicidade()
    {
        var usuario = UsuarioExistente("ana@x.com");

        await _sut.AtualizarAsync(usuario.Id, new AtualizarUsuarioRequest("Ana Maria", "ANA@x.com"), Ct);

        await _repository.DidNotReceive().ExisteEmailAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-06 (falha)
    [Fact]
    public async Task AtualizarAsync_EmailDeOutroUsuario_LancaConflictExceptionSemAlterar()
    {
        var usuario = UsuarioExistente();
        EmailJaExiste();

        var act = () => _sut.AtualizarAsync(usuario.Id, new AtualizarUsuarioRequest("Ana", "outra@x.com"), Ct);

        await act.Should().ThrowAsync<ConflictException>();
        usuario.Email.Should().Be("ana@x.com");
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-08 (falha)
    [Fact]
    public async Task ObterAsync_IdInexistente_LancaNotFoundException()
    {
        var act = () => _sut.ObterAsync(Guid.NewGuid(), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // CA-08 (falha)
    [Fact]
    public async Task InativarAsync_IdInexistente_LancaNotFoundExceptionSemCommit()
    {
        var act = () => _sut.InativarAsync(Guid.NewGuid(), Ct);

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-09
    [Fact]
    public async Task InativarEAtivar_UsuarioExistente_AlteraStatusECommita()
    {
        var usuario = UsuarioExistente();

        await _sut.InativarAsync(usuario.Id, Ct);
        usuario.Ativo.Should().BeFalse();

        await _sut.AtivarAsync(usuario.Id, Ct);
        usuario.Ativo.Should().BeTrue();

        await _unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-01
    [Fact]
    public async Task ListarAsync_PaginaComItens_RetornaResponsesComPaginacao()
    {
        var usuario = new Usuario("Ana", "ana@x.com");
        _repository.ListarPaginadoAsync(2, 10, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Usuario>([usuario], 2, 10, 11));

        var pagina = await _sut.ListarAsync(2, 10, Ct);

        pagina.Items.Should().ContainSingle().Which.Id.Should().Be(usuario.Id);
        pagina.Should().BeEquivalentTo(new { Page = 2, PageSize = 10, TotalCount = 11 });
    }
}
