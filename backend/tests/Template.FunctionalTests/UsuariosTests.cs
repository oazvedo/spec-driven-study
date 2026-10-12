using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Dtos.Usuarios;
using Template.Domain.Common;

namespace Template.FunctionalTests;

// HTTP → API inteira → Postgres real. Cada teste começa com o banco vazio.
public class UsuariosTests(ApiFactory factory) : IAsyncLifetime
{
    private const string Url = "/api/usuarios";
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await factory.LimparBancoAsync(Ct);

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task<UsuarioResponse> CriarAsync(string nome = "Ana", string email = "ana@x.com")
    {
        var response = await _client.PostAsJsonAsync(Url, new CriarUsuarioRequest(nome, email), Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UsuarioResponse>(Ct))!;
    }

    private Task<UsuarioResponse?> ObterAsync(Guid id) => _client.GetFromJsonAsync<UsuarioResponse>($"{Url}/{id}", Ct);

    // CA-01
    [Fact]
    public async Task Listar_SemParametros_Retorna20ItensETotal()
    {
        for (var i = 1; i <= 25; i++)
            await CriarAsync($"Usuário {i}", $"u{i}@x.com");

        var pagina = await _client.GetFromJsonAsync<PagedResult<UsuarioResponse>>(Url, Ct);

        pagina!.Items.Should().HaveCount(20);
        pagina.Should().BeEquivalentTo(new { Page = 1, PageSize = 20, TotalCount = 25, TotalPages = 2 });
    }

    // CA-02 (falha)
    [Theory]
    [InlineData("pageSize=101", "pageSize")]
    [InlineData("page=0", "page")]
    public async Task Listar_ParametrosForaDoLimite_Retorna400(string query, string campo)
    {
        var response = await _client.GetAsync($"{Url}?{query}", Ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        problem!.Errors.Should().ContainKey(campo);
    }

    // (page - 1) * pageSize passaria do int: a página além do fim volta vazia, sem 500
    [Fact]
    public async Task Listar_PaginaMaxima_Retorna200Vazio()
    {
        await CriarAsync();

        var pagina = await _client.GetFromJsonAsync<PagedResult<UsuarioResponse>>($"{Url}?page={int.MaxValue}", Ct);

        pagina!.Items.Should().BeEmpty();
        pagina.TotalCount.Should().Be(1);
    }

    // ADR 0006: o que falha no binding (antes do service) também responde em português
    [Fact]
    public async Task Listar_PageNaoNumerico_Retorna400EmPortugues()
    {
        var response = await _client.GetAsync($"{Url}?page=abc", Ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        problem!.Title.Should().Be("Dados inválidos.");
        problem.Errors["page"].Should().Equal("O valor 'abc' não é válido.");
    }

    [Fact]
    public async Task Criar_JsonMalformado_Retorna400EmPortugues()
    {
        using var corpo = new StringContent("{oops", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(Url, corpo, Ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        problem!.Title.Should().Be("Dados inválidos.");
        problem.Errors.Values.SelectMany(x => x).Should().Equal("O JSON enviado não é válido.");
    }

    // CA-03
    [Fact]
    public async Task Criar_RequestValido_Retorna201ComLocation()
    {
        var response = await _client.PostAsJsonAsync(Url, new CriarUsuarioRequest("Ana", "Ana@X.com"), Ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<UsuarioResponse>(Ct);
        criado!.Should().BeEquivalentTo(new { Nome = "Ana", Email = "ana@x.com", Ativo = true });
        response.Headers.Location!.AbsolutePath.Should().Be($"{Url}/{criado.Id}");
    }

    // CA-04 (falha)
    [Fact]
    public async Task Criar_CamposVazios_Retorna400ComErrosPorCampo()
    {
        var response = await _client.PostAsJsonAsync(Url, new CriarUsuarioRequest("", ""), Ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        problem!.Errors.Should().ContainKeys("nome", "email");
        (await _client.GetFromJsonAsync<PagedResult<UsuarioResponse>>(Url, Ct))!.TotalCount.Should().Be(0);
    }

    // CA-06 (falha)
    [Theory]
    [InlineData("ana@x.com")]
    [InlineData("ANA@x.com")]
    public async Task Criar_EmailDuplicado_Retorna409(string email)
    {
        await CriarAsync(email: "ana@x.com");

        var response = await _client.PostAsJsonAsync(Url, new CriarUsuarioRequest("Outra Ana", email), Ct);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // CA-07
    [Fact]
    public async Task Atualizar_RequestValido_Retorna200ComDadosNovos()
    {
        var usuario = await CriarAsync();

        var response = await _client.PutAsJsonAsync($"{Url}/{usuario.Id}",
            new AtualizarUsuarioRequest("Ana Maria", "anamaria@x.com"), Ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ObterAsync(usuario.Id)).Should().BeEquivalentTo(new { Nome = "Ana Maria", Email = "anamaria@x.com" });
    }

    // CA-08 (falha)
    [Fact]
    public async Task Obter_IdInexistente_Retorna404()
        => (await _client.GetAsync($"{Url}/{Guid.NewGuid()}", Ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);

    // CA-08 (falha)
    [Fact]
    public async Task Atualizar_IdInexistente_Retorna404()
        => (await _client.PutAsJsonAsync($"{Url}/{Guid.NewGuid()}", new AtualizarUsuarioRequest("Ana", "ana@x.com"), Ct))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

    // CA-08 (falha)
    [Theory]
    [InlineData("ativar")]
    [InlineData("inativar")]
    public async Task AtivarOuInativar_IdInexistente_Retorna404(string acao)
        => (await _client.PatchAsync($"{Url}/{Guid.NewGuid()}/{acao}", null, Ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);

    // CA-09
    [Fact]
    public async Task InativarEAtivar_UsuarioExistente_Retorna204EAlteraStatus()
    {
        var usuario = await CriarAsync();

        (await _client.PatchAsync($"{Url}/{usuario.Id}/inativar", null, Ct)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterAsync(usuario.Id))!.Ativo.Should().BeFalse();

        (await _client.PatchAsync($"{Url}/{usuario.Id}/ativar", null, Ct)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterAsync(usuario.Id))!.Ativo.Should().BeTrue();
    }
}
