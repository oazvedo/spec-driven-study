using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Domain.Entities;
using Template.Domain.Entities.Enums;
using Template.Domain.Entities.Interfaces;

namespace Template.IntegrationTests.Repositories;

// Cada etapa usa um escopo (DbContext) novo, como requisições diferentes, para ler o que foi de fato gravado
public class EnderecoRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Guid _usuarioId;

    public async ValueTask InitializeAsync()
    {
        await fixture.LimparBancoAsync(Ct);

        await using var scope = fixture.Services.CreateAsyncScope();
        var usuario = new Usuario("Ana", "ana@x.com");
        await scope.ServiceProvider.GetRequiredService<IUsuarioRepository>().AdicionarAsync(usuario, Ct);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync(Ct);
        _usuarioId = usuario.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<T> NoEscopo<T>(Func<IEnderecoRepository, IUnitOfWork, Task<T>> acao)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await acao(
            scope.ServiceProvider.GetRequiredService<IEnderecoRepository>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }

    private static Endereco NovoEndereco(Guid usuarioId, string logradouro = "Av. Paulista")
        => new(usuarioId, logradouro, "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Comercial);

    private Task<Endereco> SalvarAsync(Endereco endereco) => NoEscopo(async (repo, uow) =>
    {
        await repo.AdicionarAsync(endereco, Ct);
        await uow.CommitAsync(Ct);
        return endereco;
    });

    private Task<Endereco?> ObterAsync(Guid id) => NoEscopo((repo, _) => repo.ObterPorIdAsync(id, Ct));

    [Fact]
    public async Task Adicionar_Commit_GravaAtivoComOsDados()
    {
        var endereco = await SalvarAsync(NovoEndereco(_usuarioId));

        var salvo = await ObterAsync(endereco.Id);

        salvo!.UsuarioId.Should().Be(_usuarioId);
        salvo.Cep.Should().Be("01310-100");
        salvo.Tipo.Should().Be(EnderecoTipo.Comercial);
        salvo.Ativo.Should().BeTrue();
    }

    // CA-01
    [Fact]
    public async Task ListarPaginadoAsync_25Enderecos_RetornaPrimeiraPaginaComTotal()
    {
        await NoEscopo(async (repo, uow) =>
        {
            for (var i = 1; i <= 25; i++)
                await repo.AdicionarAsync(NovoEndereco(_usuarioId, $"Rua {i}"), Ct);
            return await uow.CommitAsync(Ct);
        });

        var pagina1 = await NoEscopo((repo, _) => repo.ListarPaginadoAsync(1, 20, Ct));

        pagina1.Items.Should().HaveCount(20);
        pagina1.TotalCount.Should().Be(25);
        pagina1.TotalPages.Should().Be(2);
    }

    // CA-10 (ADR 0015 e 0016)
    [Fact]
    public async Task Remover_Commit_EnderecoNaoExisteMais()
    {
        var endereco = await SalvarAsync(NovoEndereco(_usuarioId));

        await NoEscopo(async (repo, uow) =>
        {
            repo.Remover((await repo.ObterPorIdAsync(endereco.Id, Ct))!);
            return await uow.CommitAsync(Ct);
        });

        (await ObterAsync(endereco.Id)).Should().BeNull();
    }

    // RN-01 (falha): a FK barra um usuário inexistente mesmo sem passar pela checagem do service
    [Fact]
    public async Task Commit_UsuarioInexistente_LancaDbUpdateException()
    {
        var act = () => SalvarAsync(NovoEndereco(Guid.NewGuid()));

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
