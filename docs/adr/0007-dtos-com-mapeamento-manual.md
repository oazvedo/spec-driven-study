# 0007. DTOs com mapeamento manual

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
As entidades não podem sair pela API (para não vazar o modelo nem permitir overposting). É preciso
converter entre entidade e DTO de forma **simples de ler**.

## Decisão
- DTOs como `record` em `Application/Dtos/<Entidade>s/`:
  `UsuarioResponse`, `CriarUsuarioRequest` e `AtualizarUsuarioRequest`.
- O mapeamento é **manual**, com um método de extensão no mesmo arquivo do response:
  ```csharp
  public record UsuarioResponse(Guid Id, string Nome, string Email, bool Ativo, DateTime CreatedAt);

  public static class UsuarioMappings
  {
      public static UsuarioResponse ToResponse(this Usuario u)
          => new(u.Id, u.Nome, u.Email, u.Ativo, u.CreatedAt);
  }
  ```
- Request para entidade: o service chama o construtor ou os métodos da entidade
  (`new Usuario(request.Nome, request.Email)`, `usuario.Atualizar(...)`). Não existe "map de request para entidade".

## Alternativas consideradas
- **AutoMapper:** menos código repetido, mas o mapeamento fica implícito, os erros só aparecem
  em runtime, e a partir da v15 a licença passou a ser comercial.
- **Mapster ou Mapperly (source generators):** boas opções caso o volume de mapeamentos cresça.
  Podem entrar em um ADR futuro.

## Consequências
- ✅ O mapeamento é visível com "go to definition", e o compilador acusa campos novos.
- ✅ Nenhuma dependência extra.
- ⚠️ Algumas linhas repetitivas por entidade.
