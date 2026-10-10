using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Domain.Common.Exceptions;
using Template.Domain.Entities;
using Template.Domain.Entities.Interfaces;

namespace Template.IntegrationTests.Repositories;

// Cada etapa usa um escopo (DbContext) novo, como requisições diferentes, para ler o que foi de fato gravado
public class UsuarioRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.LimparBancoAsync(Ct);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task<T> NoEscopo<T>(Func<IUsuarioRepository, IUnitOfWork, Task<T>> acao)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await acao(
            scope.ServiceProvider.GetRequiredService<IUsuarioRepository>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }

    private Task<Usuario> SalvarAsync(Usuario usuario) => NoEscopo(async (repo, uow) =>
    {
        await repo.AdicionarAsync(usuario, Ct);
        await uow.CommitAsync(Ct);
        return usuario;
    });

    private Task<Usuario?> ObterAsync(Guid id) => NoEscopo((repo, _) => repo.ObterPorIdAsync(id, Ct));

    [Fact]
    public async Task Adicionar_Commit_GravaComCreatedAtPreenchido()
    {
        var usuario = await SalvarAsync(new Usuario("Ana", "ana@x.com"));

        var salvo = await ObterAsync(usuario.Id);

        salvo!.Email.Should().Be("ana@x.com");
        salvo.Ativo.Should().BeTrue();
        salvo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        salvo.UpdatedAt.Should().BeNull();
    }

    // CA-01
    [Fact]
    public async Task ListarPaginadoAsync_25Registros_RetornaPrimeiraPaginaComTotal()
    {
        await NoEscopo(async (repo, uow) =>
        {
            for (var i = 1; i <= 25; i++)
                await repo.AdicionarAsync(new Usuario($"Usuário {i}", $"u{i}@x.com"), Ct);
            return await uow.CommitAsync(Ct);
        });

        var pagina1 = await NoEscopo((repo, _) => repo.ListarPaginadoAsync(1, 20, Ct));
        var pagina2 = await NoEscopo((repo, _) => repo.ListarPaginadoAsync(2, 20, Ct));

        pagina1.Items.Should().HaveCount(20);
        pagina1.TotalCount.Should().Be(25);
        pagina1.TotalPages.Should().Be(2);
        pagina2.Items.Should().HaveCount(5);
        // Mesmo CreatedAt (um commit só): o desempate pelo Id mantém as páginas sem sobreposição
        pagina1.Items.Select(x => x.Id).Should().NotIntersectWith(pagina2.Items.Select(x => x.Id));
    }

    // RN-05: os cadastrados mais recentemente aparecem primeiro
    [Fact]
    public async Task ListarPaginadoAsync_CommitsDiferentes_MaisRecentePrimeiro()
    {
        await SalvarAsync(new Usuario("Antigo", "antigo@x.com"));
        await SalvarAsync(new Usuario("Recente", "recente@x.com"));

        var pagina = await NoEscopo((repo, _) => repo.ListarPaginadoAsync(1, 20, Ct));

        pagina.Items.Select(x => x.Nome).Should().Equal("Recente", "Antigo");
    }

    [Fact]
    public async Task Atualizar_Commit_PreencheUpdatedAtEMantemCreatedAt()
    {
        var usuario = await SalvarAsync(new Usuario("Ana", "ana@x.com"));
        var createdAt = (await ObterAsync(usuario.Id))!.CreatedAt;

        await NoEscopo(async (repo, uow) =>
        {
            var salvo = await repo.ObterPorIdAsync(usuario.Id, Ct);
            salvo!.Atualizar("Ana Maria", "ana@x.com");
            repo.Atualizar(salvo);
            return await uow.CommitAsync(Ct);
        });

        var atualizado = await ObterAsync(usuario.Id);
        atualizado!.Nome.Should().Be("Ana Maria");
        atualizado.UpdatedAt.Should().NotBeNull();
        atualizado.CreatedAt.Should().Be(createdAt);
    }

    // ADR 0015
    [Fact]
    public async Task Remover_Commit_ApagaORegistro()
    {
        var usuario = await SalvarAsync(new Usuario("Ana", "ana@x.com"));

        await NoEscopo(async (repo, uow) =>
        {
            repo.Remover((await repo.ObterPorIdAsync(usuario.Id, Ct))!);
            return await uow.CommitAsync(Ct);
        });

        (await ObterAsync(usuario.Id)).Should().BeNull();
    }

    // RN-02: o repositório normaliza o e-mail recebido
    [Theory]
    [InlineData("ana@x.com")]
    [InlineData("ANA@X.COM")]
    [InlineData("  Ana@x.com ")]
    public async Task ExisteEmailAsync_MesmoEmailEmQualquerFormato_RetornaTrue(string email)
    {
        await SalvarAsync(new Usuario("Ana", "ana@x.com"));

        (await NoEscopo((repo, _) => repo.ExisteEmailAsync(email, null, Ct))).Should().BeTrue();
    }

    [Fact]
    public async Task ExisteEmailAsync_IgnorandoOProprioUsuario_RetornaFalse()
    {
        var usuario = await SalvarAsync(new Usuario("Ana", "ana@x.com"));

        (await NoEscopo((repo, _) => repo.ExisteEmailAsync("ana@x.com", usuario.Id, Ct))).Should().BeFalse();
    }

    // CA-06 (falha): o índice único barra e-mail duplicado mesmo sem passar pela checagem do service
    // (caso de requisições simultâneas), e o UnitOfWork traduz para ConflictException (409)
    [Fact]
    public async Task Commit_EmailDuplicado_LancaConflictException()
    {
        await SalvarAsync(new Usuario("Ana", "ana@x.com"));

        var act = () => SalvarAsync(new Usuario("Outra Ana", "ANA@x.com"));

        (await act.Should().ThrowAsync<ConflictException>())
            .WithInnerException<DbUpdateException>();
    }
}
