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

## Stack
.NET 10, ASP.NET Core (Controllers), EF Core, PostgreSQL, FluentValidation, xUnit, Testcontainers,
Angular, NG-ZORRO, Vitest e Playwright.
