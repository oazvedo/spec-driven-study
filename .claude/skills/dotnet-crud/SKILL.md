---
name: dotnet-crud
description: Receita para criar ou alterar uma entidade/recurso CRUD no backend .NET deste repositório (Clean Architecture com Domain, Application, Infrastructure e Api, Repository + UnitOfWork, FluentValidation, controllers REST em /api, PagedResult). Use sempre que for adicionar uma entidade, um endpoint, um repositório, um service, um validator ou uma migration no backend.
---

# Skill: dotnet-crud

Regras completas nos ADRs 0003 a 0008 e 0010. Esta skill é a **receita passo a passo**.
Os exemplos usam `Usuario`. Para outra entidade, troque `Usuario`/`usuario`/`usuarios` pelo nome dela
(domínio em português, termos técnicos em inglês).

> A infraestrutura comum (`EntityBase`, `PagedResult`, `IRepositoryBase`, `RepositoryBase`, `IUnitOfWork`,
> `UnitOfWork`, `AppDbContext`, exceções, `GlobalExceptionHandler` e os módulos) é criada uma única vez,
> pela spec 001. Aqui só se **adiciona uma entidade** sobre essa base. Não recrie nem altere a base sem um ADR.

## Ordem dos passos (cada passo compila antes do próximo)

### 1. Domain: entidade (`Template.Domain/Entities/Usuario.cs`)
```csharp
public class Usuario : EntityBase          // EntityBase: Id, CreatedAt, UpdatedAt, Ativo, Ativar(), Inativar()
{
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    private Usuario() { }                  // EF Core

    public Usuario(string nome, string email) => Atualizar(nome, email);

    public void Atualizar(string nome, string email)
    {
        Nome = nome;
        Email = NormalizarEmail(email);
    }

    // E-mail único sem diferenciar maiúsculas de minúsculas (spec 001, RN-02)
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
```
Setters privados. As mudanças passam por métodos com nome de negócio. Nada de atributos de EF.

### 2. Domain: interface do repositório (`Template.Domain/Entities/Interfaces/IUsuarioRepository.cs`)
```csharp
public interface IUsuarioRepository : IRepositoryBase<Usuario>
{
    // Recebe o e-mail como foi digitado; a implementação normaliza (RN-02) e compara por igualdade,
    // para usar o índice único da coluna
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken cancellationToken);
}
```
Só consultas **específicas** da entidade. O CRUD básico já vem de `IRepositoryBase<T>`
(`ObterPorIdAsync`, `ListarPaginadoAsync`, `AdicionarAsync`, `Atualizar` e `Remover`).
O `Remover` faz remoção física e só deve ser usado se o `plan.md` justificar. A API não expõe `DELETE`; o padrão
continua sendo ativar e inativar (ADRs 0008 e 0015).

### 3. Infrastructure: configuração (`Template.Infrastructure/Data/Configurations/UsuarioConfig.cs`)
```csharp
public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
```
Os tamanhos máximos daqui são os mesmos usados no validator (passo 7).
O índice `(created_at DESC, id DESC)` da listagem paginada **não** vai aqui: o `AppDbContext` o cria
automaticamente para toda entidade que herda de `EntityBase`.

### 4. Infrastructure: repositório (`Template.Infrastructure/Repositories/UsuarioRepository.cs`)
```csharp
public class UsuarioRepository(AppDbContext context) : RepositoryBase<Usuario>(context), IUsuarioRepository
{
    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken cancellationToken)
    {
        // Normaliza aqui, e não em cada chamador; igualdade simples usa o índice único
        var normalizado = Usuario.NormalizarEmail(email);
        return Set.AnyAsync(x => x.Email == normalizado && x.Id != ignorarId, cancellationToken);
    }
}
```
**Nunca** chame `SaveChanges` em um repositório (ADR 0004).

### 5. Infrastructure: registro no `InfraModule.cs`
```csharp
services.AddScoped<IUsuarioRepository, UsuarioRepository>();
```

### 6. Migration
`dotnet-ef` é uma ferramenta local do repositório (`dotnet-tools.json`); se não estiver instalada, rode `dotnet tool restore`.
```bash
dotnet ef migrations add AdicionarUsuario \
  -p backend/src/Template.Infrastructure -s backend/src/Template.Api -o Data/Migrations
```
Revise o arquivo gerado: tabela, colunas `snake_case`, índice único e tamanhos.
**Não** rode `dotnet ef database update`: a migration é aplicada automaticamente no próximo `dotnet run` (ADR 0013).
Por isso, revise com atenção qualquer migration que remova colunas ou tabelas.

### 7. Application: DTOs e validators
`Template.Application/Dtos/Usuarios/`:
```csharp
public record CriarUsuarioRequest(string Nome, string Email);
public record AtualizarUsuarioRequest(string Nome, string Email);
public record UsuarioResponse(Guid Id, string Nome, string Email, bool Ativo, DateTime CreatedAt);

public static class UsuarioMappings
{
    public static UsuarioResponse ToResponse(this Usuario u)
        => new(u.Id, u.Nome, u.Email, u.Ativo, u.CreatedAt);
}
```
`Template.Application/Validators/CriarUsuarioRequestValidator.cs` (e o validator de `Atualizar...`):
```csharp
public class CriarUsuarioRequestValidator : AbstractValidator<CriarUsuarioRequest>
{
    public CriarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(254);
    }
}
```

### 8. Application: service
`Template.Application/Services/Interfaces/IUsuarioService.cs`:
```csharp
public interface IUsuarioService
{
    Task<PagedResult<UsuarioResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<UsuarioResponse> ObterAsync(Guid id, CancellationToken cancellationToken);
    Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken);
    Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request, CancellationToken cancellationToken);
    Task AtivarAsync(Guid id, CancellationToken cancellationToken);
    Task InativarAsync(Guid id, CancellationToken cancellationToken);
}
```
`Template.Application/Services/UsuarioService.cs`:
```csharp
public class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IUnitOfWork unitOfWork,
    IValidator<CriarUsuarioRequest> criarValidator,
    IValidator<AtualizarUsuarioRequest> atualizarValidator) : IUsuarioService
{
    public async Task<PagedResult<UsuarioResponse>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken)
        => (await usuarioRepository.ListarPaginadoAsync(page, pageSize, cancellationToken)).Map(u => u.ToResponse());

    public async Task<UsuarioResponse> ObterAsync(Guid id, CancellationToken cancellationToken)
        => (await ObterEntidadeAsync(id, cancellationToken)).ToResponse();

    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken)
    {
        await criarValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GarantirEmailUnicoAsync(request.Email, null, cancellationToken);

        var usuario = new Usuario(request.Nome, request.Email);
        await usuarioRepository.AdicionarAsync(usuario, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return usuario.ToResponse();
    }

    public async Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        await atualizarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var usuario = await ObterEntidadeAsync(id, cancellationToken);
        await GarantirEmailUnicoAsync(request.Email, id, cancellationToken);

        usuario.Atualizar(request.Nome, request.Email);
        await unitOfWork.CommitAsync(cancellationToken);
        return usuario.ToResponse();
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

    private async Task<Usuario> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken)
        => await usuarioRepository.ObterPorIdAsync(id, cancellationToken)
           ?? throw new NotFoundException($"Usuário {id} não encontrado.");

    private async Task GarantirEmailUnicoAsync(string email, Guid? ignorarId, CancellationToken cancellationToken)
    {
        if (await usuarioRepository.ExisteEmailAsync(email, ignorarId, cancellationToken))
            throw new ConflictException("Já existe um usuário com este e-mail.");
    }
}
```
Regras: validar → carregar → regra de negócio → alterar a entidade → **um** `CommitAsync` → devolver o DTO.

### 9. Application: registro no `ApplicationModule.cs`
```csharp
services.AddScoped<IUsuarioService, UsuarioService>();
```
(Os validators já são registrados por `AddValidatorsFromAssembly`.)

### 10. Api: controller (`Template.Api/Controllers/UsuariosController.cs`)
Copie o controller de referência do **ADR 0008**: `[Route("api/<recurso-plural>")]`, as 6 ações
(`Listar`, `Obter`, `Criar`, `Atualizar`, `Ativar`, `Inativar`), `[Range]` em `page` e `pageSize`,
`CreatedAtAction(nameof(Obter))` no `Criar` e `NoContent()` em ativar e inativar.
Nenhuma lógica no controller. Quando o ADR 0012 estiver implementado, adicione `[HasPermission]` em cada ação.

### 11. Testes: **sucesso e falha em cada nível** (ADR 0010)
| Projeto | Mínimo por entidade |
|---|---|
| `Template.UnitTests` | Service: criar válido chama `AdicionarAsync` + `CommitAsync`. **Falha:** request inválido lança `ValidationException` e `CommitAsync` não é chamado; e-mail duplicado lança `ConflictException`; id inexistente lança `NotFoundException`. Validator: um caso válido e um inválido por regra. |
| `Template.IntegrationTests` | Repositório: adicionar + commit + `ObterPorIdAsync`; `ListarPaginadoAsync` com total correto. **Falha:** violar o índice único lança `DbUpdateException`. |
| `Template.FunctionalTests` | `POST` → 201 + `Location`; `GET` por id → 200; `PATCH /inativar` → 204. **Falha:** payload inválido → 400 com `errors`; `pageSize=101` → 400; id inexistente → 404; e-mail duplicado → 409. |

Exemplo de teste de unidade de falha:
```csharp
[Fact]
public async Task CriarAsync_EmailInvalido_LancaValidationException()
{
    var request = new CriarUsuarioRequest("Ana", "nao-e-email");

    var act = () => _sut.CriarAsync(request, CancellationToken.None);

    await act.Should().ThrowAsync<ValidationException>();
    await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
}
```

## Checklist de pronto
- [ ] Entidade com setters privados e métodos de negócio; sem dependência de EF
- [ ] `IXxxRepository : IRepositoryBase<Xxx>` em `Entities/Interfaces/`
- [ ] `XxxConfig` com tabela, tamanhos e índices; migration revisada
- [ ] Repositório registrado no `InfraModule`; service registrado no `ApplicationModule`
- [ ] Validators com mensagens em português e limites iguais aos da `XxxConfig`
- [ ] Service com exatamente um `CommitAsync` por operação de escrita
- [ ] Controller com as 6 ações REST, sem lógica
- [ ] Testes de unidade, integração e funcional com sucesso **e** falha; `dotnet test backend` verde
- [ ] Testes de arquitetura continuam verdes
- [ ] `dotnet run` aplica a migration sem erro, e os endpoints aparecem no `/scalar`
