# 0012. Autorização por permissão com HasPermission

- **Status:** Aceito. Será implementado na spec `002-autenticacao-permissoes`.
- **Data:** 2026-10-10

## Contexto
Os endpoints precisam de controle de acesso granular por recurso e ação, e não só por papel (role).

## Decisão
- Autenticação por **JWT bearer**.
- Autorização por **permissão**, com o atributo `[HasPermission("<recurso>.<acao>")]` em cada ação do controller:
  - `<recurso>.visualizar` nas ações `GET`;
  - `<recurso>.editar` em `POST`, `PUT` e `PATCH`.
  ```csharp
  [HttpGet]
  [HasPermission("usuarios.visualizar")]
  public Task<PagedResult<UsuarioResponse>> Listar(...) => ...;
  ```
- Implementação prevista: `HasPermissionAttribute : AuthorizeAttribute` (policy `"perm:<nome>"`),
  um `IAuthorizationPolicyProvider` que cria a policy sob demanda e um `AuthorizationHandler`
  que verifica a claim `permission` do token.
- Sem a permissão, a resposta é 403. Sem token, 401. Os dois em ProblemDetails.
- Os detalhes (de onde vêm as permissões, login e refresh) serão definidos no `plan.md` da spec 002.

## Alternativas consideradas
- **Roles (`[Authorize(Roles = ...)]`):** grosseiras demais. Mudar o acesso exigiria mudar o código.
- **Policies escritas à mão para cada permissão:** muito código repetitivo em `Program.cs`.

## Consequências
- ✅ O controle de acesso fica declarativo e visível em cada endpoint.
- ⚠️ Até a spec 002, a API fica **sem autenticação**. Ela não deve ser exposta fora do ambiente local.
