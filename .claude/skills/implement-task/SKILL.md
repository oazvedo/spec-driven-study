---
name: implement-task
description: Implementa a próxima task aberta de um docs/specs/NNN-nome/tasks.md, uma por vez, roda a verificação e marca como concluída. Use quando o usuário pedir "implemente a próxima task", "continue a spec NNN", "faça a task X", ou quiser avançar a implementação de uma spec aprovada.
---

# Skill: implement-task

## Passos
1. **Contexto.** Leia o `spec.md`, o `plan.md` e o `tasks.md` da spec, e também os ADRs citados no plan.
   Se a spec não estiver `Aprovada`, avise o usuário e pergunte se ele quer seguir mesmo assim.
2. **Escolha a task.** A primeira com `- [ ]`, a não ser que o usuário tenha indicado outra.
   Anuncie qual vai fazer e qual é o critério de verificação.
3. **Verifique a coerência.** Se a task contradiz a spec, o plan ou um ADR, ou depende de algo que não existe,
   **pare** e explique. Não improvise.
4. **Implemente só essa task.** Siga `dotnet-crud` e `angular-crud` quando se aplicarem, e as diretrizes da
   skill `andrej-karpathy-skills:karpathy-guidelines` (mudanças cirúrgicas, nada além do pedido).
   - Os testes da task fazem parte da task, incluindo o cenário de **sucesso** e o de **falha** (ADR 0010).
5. **Verifique.** Rode o comando de verificação da task (`dotnet build`, `dotnet test`, `npm test`, `npm run lint`...).
   Se falhar, corrija e rode de novo. Nunca marque como concluída uma task com verificação vermelha.
6. **Limpe.** Rode a skill `simplify` sobre o que mudou nesta task.
7. **Marque** `- [x]` no `tasks.md`. Se descobriu trabalho novo, adicione uma task nova em vez de inflar a atual.
8. **Reporte** ao usuário em até 10 linhas:
   - o que foi feito e os arquivos principais;
   - a saída resumida da verificação;
   - **o que dá para aprender aqui**: o conceito central da task em 1 ou 2 frases;
   - qual é a próxima task.
9. **Pare.** Não emende a próxima task sem o usuário pedir.

## Ao concluir a última task da spec
- Rode a skill `code-review` sobre o diff da spec inteira.
- Confira se cada `CA-xx` do `spec.md` é coberto por um teste que passa.
- Mude o status da spec para `Concluída` e atualize o roadmap.
- Sugira registrar o aprendizado com a skill `study-log`.
