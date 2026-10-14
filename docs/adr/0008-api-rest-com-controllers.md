# 0008. API REST com Controllers

- **Status:** Aceito (a regra "Sem `DELETE`" foi substituída pelo [ADR 0016](0016-delete-na-api-para-entidades-sem-historico.md))
- **Data:** 2026-10-10

## Contexto
Os endpoints precisam de um padrão REST único, previsível para o frontend e fácil de replicar para novas entidades.

## Decisão
- **Controllers** com `[ApiController]`, injeção pelo **construtor primário** e ações finas que só delegam ao service.
- Rotas em `/api/<recurso-no-plural>`, com o id tipado `{id:guid}`.
- **Sem `DELETE`**: a exclusão é lógica. Toda entidade tem `Ativo` e os métodos de domínio `Ativar()` e `Inativar()`.
- Toda ação recebe `CancellationToken` e o repassa até o EF Core.
- Os nomes das ações e dos métodos de service seguem o domínio, em português:
  `Listar`/`ListarAsync`, `Obter`/`ObterAsync`, `Criar`/`CriarAsync`, `Atualizar`/`AtualizarAsync`,
  `Ativar`/`AtivarAsync` e `Inativar`/`InativarAsync`.

| Ação | Verbo e rota | Sucesso | Erros |
|---|---|---|---|
| Listar | `GET /api/usuarios?page=1&pageSize=20` | 200 `PagedResult<UsuarioResponse>` | 400 |
| Obter | `GET /api/usuarios/{id:guid}` | 200 `UsuarioResponse` | 404 |
| Criar | `POST /api/usuarios` | 201 + `Location` | 400, 409 |
| Atualizar | `PUT /api/usuarios/{id:guid}` | 200 `UsuarioResponse` | 400, 404, 409 |
| Ativar | `PATCH /api/usuarios/{id:guid}/ativar` | 204 | 404 |
| Inativar | `PATCH /api/usuarios/{id:guid}/inativar` | 204 | 404 |

Controller de referência:
```csharp
[ApiController]
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<UsuarioResponse>> Listar(
        [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")] int page = 1,
        [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => usuarioService.ListarAsync(page, pageSize, cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<UsuarioResponse> Obter(Guid id, CancellationToken cancellationToken)
        => usuarioService.ObterAsync(id, cancellationToken);

    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar(CriarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:guid}")]
    public Task<UsuarioResponse> Atualizar(Guid id, AtualizarUsuarioRequest request, CancellationToken cancellationToken)
        => usuarioService.AtualizarAsync(id, request, cancellationToken);

    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
    {
        await usuarioService.AtivarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/inativar")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken cancellationToken)
    {
        await usuarioService.InativarAsync(id, cancellationToken);
        return NoContent();
    }
}
```
Quando a spec 003 for implementada, cada ação ganha `[HasPermission("<recurso>.visualizar|editar")]` (ADR 0012).

- **Documentação da API:** o documento OpenAPI é gerado pelo `Microsoft.AspNetCore.OpenApi` (nativo, em `/openapi/v1.json`),
  e a interface de documentação é o **Scalar** (`Scalar.AspNetCore`, em `/scalar`). Os dois ficam disponíveis só em Development:
  ```csharp
  builder.Services.AddOpenApi();
  // ...
  if (app.Environment.IsDevelopment())
  {
      app.MapOpenApi();
      app.MapScalarApiReference();
  }
  ```
  Usamos o Scalar em vez do Swagger UI porque o Swashbuckle deixou de vir nos templates desde o .NET 9,
  e o Scalar funciona direto sobre o OpenAPI nativo, sem gerador extra.

## Alternativas consideradas
- **Minimal APIs:** menos cerimônia, mas o padrão Controller + Service é mais familiar e já é o modelo adotado.
- **`DELETE` físico:** perde histórico e quebra referências. A exclusão lógica atende o CRUD.

## Consequências
- ✅ Toda entidade tem os mesmos 6 endpoints, e o frontend reutiliza o mesmo padrão de API.
- ✅ As ações cabem em 1 a 4 linhas, e as regras ficam no service.
- ⚠️ As listagens precisam decidir se mostram inativos. O padrão é mostrar todos, com a coluna "Ativo".
