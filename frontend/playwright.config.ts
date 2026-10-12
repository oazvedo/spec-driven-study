import { defineConfig } from '@playwright/test';

// E2E com API e banco reais (ADR 0010). Pré-requisito: `docker compose up -d` (Postgres).
// A API e o `ng serve` sobem sozinhos; se já estiverem rodando, são reaproveitados.
export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command: 'dotnet run --project ../backend/src/Template.Api --launch-profile http --no-launch-browser',
      url: 'http://localhost:5000/api/health',
      reuseExistingServer: true,
      timeout: 180_000,
    },
    {
      command: 'npm start',
      url: 'http://localhost:4200',
      reuseExistingServer: true,
      timeout: 180_000,
    },
  ],
});
