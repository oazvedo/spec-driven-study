using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Dtos.Enderecos;
using Template.Application.Dtos.Usuarios;
using Template.Domain.Common;
using Template.Domain.Entities.Enums;

namespace Template.FunctionalTests;

// HTTP → API inteira → Postgres real. Cada teste começa com o banco vazio e um usuário dono dos endereços.
public class EnderecosTests(ApiFactory factory) : IAsyncLifetime
{
    private const string Url = "/api/enderecos";
    private readonly HttpClient _client = factory.CreateClient();
    private Guid _usuarioId;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await factory.LimparBancoAsync(Ct);
        _usuarioId = await CriarUsuarioAsync("ana@x.com");
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task<Guid> CriarUsuarioAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/usuarios", new CriarUsuarioRequest("Ana", email), Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UsuarioResponse>(Ct))!.Id;
    }

    private CriarEnderecoRequest Request(string logradouro = "Av. Paulista")
        => new(_usuarioId, logradouro, "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Comercial);

    private async Task<EnderecoResponse> CriarAsync(string logradouro = "Av. Paulista")
    {
        var response = await _client.PostAsJsonAsync(Url, Request(logradouro), Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EnderecoResponse>(Ct))!;
    }

    private Task<EnderecoResponse?> ObterAsync(Guid id) => _client.GetFromJsonAsync<EnderecoResponse>($"{Url}/{id}", Ct);

    // CA-01
    [Fact]
    public async Task Listar_SemParametros_Retorna20ItensETotal()
    {
        for (var i = 1; i <= 25; i++)
            await CriarAsync($"Rua {i}");

        var pagina = await _client.GetFromJsonAsync<PagedResult<EnderecoResponse>>(Url, Ct);

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

    // CA-03
    [Fact]
    public async Task Criar_RequestValido_Retorna201ComLocationEAtivo()
    {
        var response = await _client.PostAsJsonAsync(Url, Request(), Ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<EnderecoResponse>(Ct);
        criado!.Should().BeEquivalentTo(new { UsuarioId = _usuarioId, Logradouro = "Av. Paulista", Tipo = EnderecoTipo.Comercial, Ativo = true });
        response.Headers.Location!.AbsolutePath.Should().Be($"{Url}/{criado.Id}");
    }

    // CA-04 (falha)
    [Fact]
    public async Task Criar_CamposVazios_Retorna400ComErrosPorCampo()
    {
        var response = await _client.PostAsJsonAsync(Url,
            Request() with { Logradouro = "", Numero = "", Bairro = "", Cidade = "", Estado = "", Cep = "" }, Ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        problem!.Errors.Should().ContainKeys("logradouro", "numero", "bairro", "cidade", "estado", "cep");
        (await _client.GetFromJsonAsync<PagedResult<EnderecoResponse>>(Url, Ct))!.TotalCount.Should().Be(0);
    }

    // CA-06 (falha)
    [Fact]
    public async Task Criar_UsuarioInexistente_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(Url, Request() with { UsuarioId = Guid.NewGuid() }, Ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _client.GetFromJsonAsync<PagedResult<EnderecoResponse>>(Url, Ct))!.TotalCount.Should().Be(0);
    }

    // CA-07: inclui trocar o dono do endereço
    [Fact]
    public async Task Atualizar_RequestValido_Retorna200ComDadosNovos()
    {
        var endereco = await CriarAsync();
        var outroUsuarioId = await CriarUsuarioAsync("bia@x.com");

        var response = await _client.PutAsJsonAsync($"{Url}/{endereco.Id}", new AtualizarEnderecoRequest(
            outroUsuarioId, "Rua Augusta", "500", "Apto 12", "Consolação", "São Paulo", "SP", "01305000", EnderecoTipo.Residencial), Ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ObterAsync(endereco.Id)).Should().BeEquivalentTo(new
        {
            UsuarioId = outroUsuarioId, Logradouro = "Rua Augusta", Complemento = "Apto 12", Tipo = EnderecoTipo.Residencial
        });
    }

    // CA-08 (falha)
    [Fact]
    public async Task Obter_IdInexistente_Retorna404()
        => (await _client.GetAsync($"{Url}/{Guid.NewGuid()}", Ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);

    // CA-08 (falha)
    [Fact]
    public async Task Atualizar_IdInexistente_Retorna404()
        => (await _client.PutAsJsonAsync($"{Url}/{Guid.NewGuid()}", new AtualizarEnderecoRequest(
                _usuarioId, "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Comercial), Ct))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

    // CA-08 (falha)
    [Theory]
    [InlineData("PATCH", "/ativar")]
    [InlineData("PATCH", "/inativar")]
    [InlineData("DELETE", "")]
    public async Task AtivarInativarExcluir_IdInexistente_Retorna404(string verbo, string sufixo)
    {
        using var request = new HttpRequestMessage(new HttpMethod(verbo), $"{Url}/{Guid.NewGuid()}{sufixo}");

        (await _client.SendAsync(request, Ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // CA-09
    [Fact]
    public async Task InativarEAtivar_EnderecoExistente_Retorna204EAlteraStatus()
    {
        var endereco = await CriarAsync();

        (await _client.PatchAsync($"{Url}/{endereco.Id}/inativar", null, Ct)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterAsync(endereco.Id))!.Ativo.Should().BeFalse();

        (await _client.PatchAsync($"{Url}/{endereco.Id}/ativar", null, Ct)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ObterAsync(endereco.Id))!.Ativo.Should().BeTrue();
    }

    // CA-10 (ADR 0016)
    [Fact]
    public async Task Excluir_EnderecoExistente_Retorna204EDepois404()
    {
        var endereco = await CriarAsync();

        (await _client.DeleteAsync($"{Url}/{endereco.Id}", Ct)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.GetAsync($"{Url}/{endereco.Id}", Ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
