# 0015. Remoção física disponível no repositório

- **Status:** Aceito
- **Data:** 2026-10-10

## Contexto
O ADR 0004 definiu que não haveria remoção física: toda exclusão seria lógica (`Ativar()` e `Inativar()`).
Algumas entidades, porém, não precisam de histórico, como itens filhos de um agregado ou tabelas de ligação.
Para elas, inativar só acumularia lixo no banco.

## Decisão
- O `IRepositoryBase<T>` ganha a operação de remoção física, simétrica ao `Atualizar`:
  ```csharp
  void Remover(T entity);
  ```
- Assim como as outras operações, ela **só marca** a entidade para remoção. O `DELETE` acontece no banco
  quando o service chama `IUnitOfWork.CommitAsync` (ADR 0004). O `IUnitOfWork` não muda.
- **A API continua sem `DELETE` (ADR 0008).** Os recursos expostos, como `Usuario`, seguem com ativar e inativar.
  Usar `Remover` é uma decisão de cada caso de uso, registrada no `plan.md` da spec que precisar dele.
- Esta decisão substitui **apenas** a frase "Não há remoção física" do ADR 0004. O restante do 0004 continua valendo.

## Alternativas consideradas
- **Remoção no `IUnitOfWork`:** misturaria dois papéis. O UoW só confirma a transação, enquanto o repositório
  é quem conhece as entidades.
- **`DELETE` em todos os endpoints:** foi descartado por você. A exclusão lógica continua sendo o padrão da API.
- **Não ter remoção física:** obrigaria a inativar até mesmo os dados sem valor histórico.

## Consequências
- ✅ Entidades sem histórico podem ser apagadas de verdade, sem gambiarra.
- ⚠️ Como fica na base, **todo** repositório herda `Remover`, inclusive o de entidades que só devem ser inativadas
  (como `Usuario`). Nada no tipo impede o uso indevido; a barreira é a revisão (o `plan.md` precisa justificar o uso).
  Se isso virar problema, a alternativa é mover `Remover` para uma interface opcional implementada só pelos repositórios que precisam dela.
- ⚠️ `Remover` em uma entidade com chave estrangeira pode falhar, com `DbUpdateException`, ou apagar em cascata,
  conforme a `XxxConfig`. Defina o `OnDelete` de forma explícita na configuração quando usar `Remover`.
