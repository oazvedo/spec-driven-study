# CLAUDE.md

Repositório de estudos: CRUD em **C# (.NET 10) + Angular**, construído com IA seguindo um fluxo spec-driven.
Documentação em PT-BR. Termos de domínio no código em português; termos técnicos em inglês.

## Antes de qualquer coisa
1. Leia os ADRs em `docs/adr/` (índice em `docs/adr/README.md`). **Eles são lei.** Se uma tarefa exigir
   contrariar um ADR, pare e proponha um novo ADR (skill `adr`). Não desvie em silêncio.
2. Trabalhe a partir de uma spec em `docs/specs/NNN-nome/`. Sem spec, crie uma (skill `spec`).
3. Implemente **uma task por vez** (skill `implement-task`).

## Skills do projeto (`.claude/skills/`)
| Skill | Quando usar |
|---|---|
| `spec` | Nova feature ou mudança de comportamento: cria spec.md → plan.md → tasks.md |
| `adr` | Qualquer decisão técnica nova ou mudança de uma decisão existente |
| `implement-task` | Executar a próxima task aberta de um `tasks.md` |
| `dotnet-crud` | Criar ou alterar uma entidade/recurso CRUD no backend |
| `angular-crud` | Criar ou alterar uma feature CRUD no frontend |
| `study-log` | Ao concluir uma task ou spec: registrar o que foi aprendido |

## Skills globais e quando usar
- `andrej-karpathy-skills:karpathy-guidelines`: toda tarefa de código.
- `simplify`: **obrigatória** depois de toda implementação, antes de marcar como concluída ou commitar. Nunca pule, nem em mudanças pequenas.
- `code-review`: ao terminar todas as tasks de uma spec.
- `security-review`: ao mexer em input do usuário, acesso a dados, autenticação ou permissões.
- `better-accessibility` e `better-writing`: em telas Angular (a11y e textos de interface).
- `anthropic-skills:skill-creator`: para criar ou ajustar skills deste projeto.

## Regras rápidas (detalhes nos ADRs)
- **Backend:** Clean Architecture com Domain, Application, Infrastructure e Api (ADR 0003). DI só em
  `ApplicationModule.cs` e `InfraModule.cs` (ADR 0003). Repository + UnitOfWork + `PagedResult<T>` (ADR 0004).
  PostgreSQL + EF Core com uma `XxxConfig` por entidade (ADR 0005). FluentValidation + ProblemDetails (ADR 0006).
  DTOs com mapeamento manual (ADR 0007). Controllers REST em `/api/<recurso>`, sem DELETE, documentação
  com Scalar em `/scalar` (ADR 0008). As migrations são aplicadas automaticamente quando a API inicia (ADR 0013).
- **Frontend:** Angular com Feature-Sliced Design + NG-ZORRO (ADR 0009).
- **Testes:** unidade, integração, funcional, e2e e arquitetura, sempre com um cenário de sucesso
  e um de falha (ADR 0010).

## Git
- Commits **sem** `Co-Authored-By` nem qualquer atribuição ao Claude.
- Só commite quando o usuário pedir.

## Ambiente
- O repositório fica no WSL (Ubuntu). **Rode `dotnet` dentro do WSL** (`~/.dotnet`, carregado pelo `~/.bashrc`).
  O SDK do Windows sobre `\\wsl.localhost\...` compila, mas o Smart App Control bloqueia as DLLs na hora de rodar os testes.
  A partir do Claude Code no Windows: `wsl.exe -d Ubuntu -e bash -ic 'cd ~/projects/template-crud-skills && <comando>'`.
- Os testes usam o Microsoft.Testing.Platform (`global.json` na raiz) com xUnit v3.
- Os testes com Testcontainers precisam do Docker acessível no WSL (Docker Desktop → Settings → Resources → WSL integration → Ubuntu).

## Comandos
```bash
docker compose up -d                       # Postgres (já disponível)
# a partir da spec 001:
dotnet run --project backend/src/Template.Api   # aplica as migrations e sobe a API; docs em /scalar
dotnet build backend && dotnet test backend
cd frontend && npm test && npm run lint && npm run e2e
```
