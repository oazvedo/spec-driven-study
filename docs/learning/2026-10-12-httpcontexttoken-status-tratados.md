# HttpContextToken: quem chama declara os erros que trata
- **Data:** 2026-10-12 · **Spec/Task:** 001 / T13, T15 · **Tags:** angular, http, interceptor

## O que é
Um `HttpContextToken` é um "metadado" tipado que viaja junto com a requisição e que os interceptors conseguem ler.
Ele tem um valor padrão, e quem faz a chamada pode sobrescrever esse valor com um `HttpContext`.
Assim, quem chama consegue mudar o comportamento de um interceptor global sem acoplar o interceptor a telas específicas.

## Por que importa aqui
O `problemDetailsInterceptor` mostra um nz-message para todo erro da API. O formulário de usuário mostra
o 409 (e-mail duplicado) no próprio campo. Sem o token, o erro aparecia duas vezes. Com `STATUS_TRATADOS`,
o formulário declara `[400, 409]` e o interceptor fica quieto para esses status. As camadas do FSD continuam
respeitadas (ADR 0009): `shared/api` não sabe que `features/usuario-form` existe.

## Onde aparece no código
- `frontend/src/app/shared/api/problem-details.interceptor.ts:10`: o token, com padrão `[]` (cada chamada declara o que trata).
- `frontend/src/app/shared/api/problem-details.interceptor.ts:19`: o interceptor lê o token.
- `frontend/src/app/features/usuario-form/usuario-form.ts:13`: o formulário declara `[400, 409]`.

## Exemplo mínimo
```ts
export const STATUS_TRATADOS = new HttpContextToken<readonly number[]>(() => []);

// no interceptor
if (!req.context.get(STATUS_TRATADOS).includes(erro.status)) mensagens.error(...);

// em quem chama
const CONTEXTO = new HttpContext().set(STATUS_TRATADOS, [400, 409]);
this.api.criar(request, CONTEXTO);
```

## Dúvidas em aberto / para aprofundar
- https://angular.dev/guide/http/interceptors#request-metadata
