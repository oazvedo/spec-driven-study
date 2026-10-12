# Playwright `webServer`: o e2e sobe o que precisa
- **Data:** 2026-10-12 · **Spec/Task:** 001 / T19 · **Tags:** angular, playwright, e2e, wsl

## O que é
A opção `webServer` do `playwright.config.ts` roda comandos antes dos testes e espera cada URL responder.
Com `reuseExistingServer: true`, se a URL já responde (você está com a API e o `ng serve` abertos),
o Playwright usa o servidor que já está de pé em vez de subir outro.

## Por que importa aqui
O ADR 0010 pede e2e com API e banco reais. O config sobe a API (`dotnet run`, que aplica as migrations
pelo ADR 0013) e o `ng serve` (que tem o proxy de `/api`), então `npm run e2e` só depende do `docker compose up -d`.
Como o banco é o mesmo do desenvolvimento, o teste de cadastro usa um e-mail único (`Date.now()`) e o teste
de falha confere que nenhum POST saiu, em vez de contar registros.

## Onde aparece no código
- `frontend/playwright.config.ts`: `webServer` com a API (espera `/api/health`) e o front.
- `frontend/e2e/usuarios.spec.ts`: os testes do CA-03 (sucesso) e do CA-04 (falha).

## Exemplo mínimo
```ts
webServer: [
  { command: 'dotnet run --project ../backend/src/Template.Api --launch-profile http --no-launch-browser',
    url: 'http://localhost:5000/api/health', reuseExistingServer: true, timeout: 180_000 },
  { command: 'npm start', url: 'http://localhost:4200', reuseExistingServer: true, timeout: 180_000 },
],
```

## Dúvidas em aberto / para aprofundar
- No WSL, o Chromium precisa das libs do sistema (`libnspr4` etc.): `sudo env "PATH=$PATH" npx playwright install-deps chromium`.
  O `sudo` não enxerga o Node do nvm sem o `env "PATH=$PATH"`.
- Vale ter um banco só para o e2e (outro `ASPNETCORE_ENVIRONMENT` e um `globalSetup`)? Hoje os testes deixam usuários no banco de dev.
- https://playwright.dev/docs/test-webserver
