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

    // Usuários que "existem" no banco falso; ObterNomesAsync devolve só os ids que estão aqui
    private readonly Dictionary<Guid, string> _usuarios = new() { [UsuarioId] = "Ana" };

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public EnderecoServiceTests()
    {
        _sut = new EnderecoService(_repository, _usuarioRepository, _unitOfWork,
            new CriarEnderecoRequestValidator(), new AtualizarEnderecoRequestValidator());
        _usuarioRepository.ObterNomesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _usuarios.Where(u => ci.Arg<IReadOnlyCollection<Guid>>().Contains(u.Key)).ToDictionary());
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

        response.Should().BeEquivalentTo(new { UsuarioId, UsuarioNome = "Ana", Ativo = true });
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
    public async Task AtualizarAsync_RequestValido_AlteraECommita()
    {
        var endereco = EnderecoExistente();

        var response = await _sut.AtualizarAsync(endereco.Id, AtualizarRequest(UsuarioId), Ct);

        response.Should().BeEquivalentTo(new { Logradouro = "Rua Augusta", Tipo = EnderecoTipo.Residencial, UsuarioNome = "Ana" });
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // CA-07: trocar o dono do endereço devolve o nome do novo dono
    [Fact]
    public async Task AtualizarAsync_OutroUsuarioExistente_TrocaOUsuario()
    {
        var endereco = EnderecoExistente();
        var outroUsuarioId = Guid.NewGuid();
        _usuarios[outroUsuarioId] = "Bia";

        var response = await _sut.AtualizarAsync(endereco.Id, AtualizarRequest(outroUsuarioId), Ct);

        response.Should().BeEquivalentTo(new { UsuarioId = outroUsuarioId, UsuarioNome = "Bia" });
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

        pagina.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new { endereco.Id, UsuarioNome = "Ana", Ativo = true });
        pagina.Should().BeEquivalentTo(new { Page = 2, PageSize = 10, TotalCount = 11 });
    }

    // Os nomes da página inteira vêm de uma consulta só, sem repetir o mesmo usuário
    [Fact]
    public async Task ListarAsync_VariosEnderecosDoMesmoUsuario_ConsultaOsNomesUmaVez()
    {
        var enderecos = new[] { EnderecoExistente(), EnderecoExistente() };
        _repository.ListarPaginadoAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Endereco>(enderecos, 1, 20, 2));

        var pagina = await _sut.ListarAsync(1, 20, Ct);

        pagina.Items.Select(x => x.UsuarioNome).Should().Equal("Ana", "Ana");
        await _usuarioRepository.Received(1).ObterNomesAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { UsuarioId })), Arg.Any<CancellationToken>());
    }
}
