# 0005. PostgreSQL + EF Core

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
É preciso um banco relacional robusto, gratuito e fácil de subir localmente e em testes.

## Decisão
- **PostgreSQL**, acessado via **EF Core** com o provider Npgsql.
- Postgres local via `docker compose` (`docker-compose.yml` na raiz). Nos testes, Testcontainers (ADR 0010).
- **Cada entidade tem uma classe de configuração** em `Infrastructure/Data/Configurations/XxxConfig.cs`,
  que descreve como ela é modelada no banco. Nada de data annotations nas entidades.
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
- O `AppDbContext` aplica todas as configurações com `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
- Nomes no banco em `snake_case`, via `EFCore.NamingConventions` (`UseSnakeCaseNamingConvention()`).
- O schema evolui por **migrations** do EF Core, versionadas em `Infrastructure/Data/Migrations/`
  e aplicadas automaticamente quando a API inicia (ADR 0013).
- Connection string em `ConnectionStrings:Default`. Segredos nunca vão para o repositório.

## Alternativas consideradas
- **SQL Server:** ótimo no ecossistema .NET, mas a imagem Docker é mais pesada e o Postgres é o padrão em open source.
- **SQLite:** simples, mas difere do Postgres em tipos e constraints, e os testes deixariam de ser fiéis.
- **Dapper:** mais controle sobre o SQL, mas exige escrever migrations e mapeamentos à mão.

## Consequências
- ✅ O mapeamento fica isolado na Infrastructure, e o Domain continua limpo.
- ✅ Os testes de integração rodam contra um Postgres real.
- ⚠️ Exige Docker instalado para desenvolver e testar.
