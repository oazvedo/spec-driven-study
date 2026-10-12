# template-crud-skills

Base de estudos para construir um CRUD em **C# (.NET 10) + Angular** usando IA de forma disciplinada:
toda mudança nasce de uma **spec**, toda decisão técnica vira um **ADR**, e a IA segue **skills**
que codificam os padrões do projeto.

## Como estudar com este repositório

```
ideia ──► spec.md ──► plan.md ──► tasks.md ──► implementa 1 task ──► study-log
          (o quê)     (como)      (passos)     (verifica + revisa)    (o que aprendi)
            ▲            │
            └── ADR ◄────┘  (quando o "como" exige uma decisão nova)
```

1. Peça ao Claude: *"use a skill spec para criar a spec 00X-<nome>"*. Revise o `spec.md` antes de seguir.
2. Revise o `plan.md`. Se ele tomar uma decisão nova, um ADR será proposto. Aceite ou rejeite o ADR.
3. Revise o `tasks.md`.
4. Para cada task: *"implemente a próxima task da spec 00X"*. Leia o diff e entenda o que mudou.
5. Ao final: *"registre no study-log"*.

## Onde está cada coisa
| Caminho | Conteúdo |
|---|---|
| `CLAUDE.md` | Regras que o Claude lê em toda sessão |
| `.claude/skills/` | Skills do projeto (padrões de backend e frontend, fluxo de specs, ADRs) |
| `docs/adr/` | Decisões arquiteturais |
| `docs/specs/` | Specs por feature (spec, plan, tasks) |
| `docs/learning/` | Diário de estudos |
| `backend/`, `frontend/` | Código (criado pela spec 001) |

## Comandos
Pré-requisitos: Docker (com a integração WSL ligada, se estiver no Windows), .NET 10 SDK e Node.

```bash
docker compose up -d                            # Postgres
dotnet tool restore                             # dotnet-ef local, uma vez por clone

# Backend: aplica as migrations ao subir; documentação em http://localhost:5000/scalar
dotnet run --project backend/src/Template.Api
dotnet build backend && dotnet test backend     # unidade, integração, funcional e arquitetura

# Frontend: http://localhost:4200 (proxy de /api para a API)
cd frontend && npm install
npm start
npm test && npm run lint                        # Vitest e lint (inclui as regras do FSD)
npm run e2e                                     # Playwright: sobe a API e o ng serve se não estiverem rodando
```

Para o e2e rodar no Linux/WSL, instale uma vez o Chromium e as bibliotecas do sistema:
`npx playwright install chromium && sudo env "PATH=$PATH" npx playwright install-deps chromium`.

## Stack
.NET 10, ASP.NET Core (Controllers), EF Core, PostgreSQL, FluentValidation, xUnit, Testcontainers,
Angular, NG-ZORRO, Vitest e Playwright.
