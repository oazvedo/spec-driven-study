# 0013. Migrations aplicadas automaticamente na inicialização

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
Rodar `dotnet ef database update` à mão depois de cada migration nova é fácil de esquecer, e a API
sobe com o schema desatualizado. Para estudar, o ciclo precisa ser: subir o Postgres, `dotnet run` e tudo funcionar.

## Decisão
- A API aplica as migrations pendentes **toda vez que inicia**, seja com `dotnet run`, seja pelo debug na IDE,
  antes de começar a aceitar requisições.
- A lógica fica na Infrastructure, como extensão, para que a Api continue sem conhecer o EF Core diretamente:
  ```csharp
  // Infrastructure/Data/MigrationExtensions.cs
  public static class MigrationExtensions
  {
      public static async Task ApplyMigrationsAsync(this IServiceProvider services)
      {
          await using var scope = services.CreateAsyncScope();
          var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
          await db.Database.MigrateAsync();
      }
  }

  // Api/Program.cs, depois de builder.Build()
  await app.Services.ApplyMigrationsAsync();
  ```
- Se o banco estiver fora do ar ou a migration falhar, a API **não sobe**. O erro aparece no console e
  é melhor falhar cedo do que rodar com o schema errado.
- Os testes funcionais herdam esse comportamento: a `WebApplicationFactory` sobe a API, e as migrations
  são aplicadas no Postgres do Testcontainers.

## Alternativas consideradas
- **`dotnet ef database update` manual:** é o mais seguro para produção, mas fácil de esquecer no dia a dia.
- **Aplicar só em Development:** o pedido é que qualquer `dotnet run` aplique. Quando houver um ambiente
  de produção, um ADR novo pode restringir esse comportamento.
- **Migration bundle (`dotnet ef migrations bundle`) no deploy:** é a abordagem certa para produção, mas o projeto ainda não tem deploy.

## Consequências
- ✅ `docker compose up -d` + `dotnet run` é tudo o que se precisa para ter o banco atualizado.
- ⚠️ Com várias instâncias da API subindo ao mesmo tempo, elas disputariam a migration. Não é um problema
  enquanto houver uma só instância local.
- ⚠️ Uma migration destrutiva é aplicada sem confirmação. Revise toda migration gerada antes de rodar a API.
