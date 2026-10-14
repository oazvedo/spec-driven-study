# 0016. DELETE na API para entidades sem histórico

- **Status:** Aceito
- **Data:** 2026-10-13

## Contexto
O ADR 0008 proíbe `DELETE` na API: toda exclusão é lógica, com ativar e inativar. O ADR 0015 liberou
a remoção física só no repositório, sem expô-la. A spec 002 traz o `Endereco`, um dado que pode ser cadastrado
por engano e não tem valor histórico. Inativar um endereço errado só deixa lixo na listagem.

## Decisão
- Um recurso **pode** expor `DELETE /api/<recurso>/{id:guid}` quando o `plan.md` da spec justificar que a entidade
  não tem valor histórico. O primeiro caso é o `Endereco` (spec 002).
- O `Usuario` e qualquer recurso sem essa justificativa continuam **sem `DELETE`** (ADR 0008).
- Quem expõe `DELETE` também mantém `ativar` e `inativar`. Excluir é uma ação a mais, não substitui as outras.
- O nome segue o domínio em português: ação `Excluir`, service `ExcluirAsync`, que chama `Remover` do repositório (ADR 0015).
- Respostas: **204** em caso de sucesso, **404** quando o registro não existe.
  ```csharp
  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
  {
      await enderecoService.ExcluirAsync(id, cancellationToken);
      return NoContent();
  }
  ```
- O frontend pede confirmação antes de excluir.
- Esta decisão substitui **apenas** a regra "Sem `DELETE`" do ADR 0008. O restante do 0008 continua valendo.

## Alternativas consideradas
- **Manter só a exclusão lógica:** os endereços cadastrados por engano ficariam para sempre na listagem como inativos,
  e o administrador não teria como limpá-los.
- **Liberar `DELETE` em todos os recursos:** o `Usuario` perderia o histórico e as referências de outras tabelas
  quebrariam. Foi o motivo do ADR 0008.
- **Excluir via `PATCH /{id}/excluir`:** evita o verbo `DELETE`, mas foge do REST sem nenhum ganho.

## Consequências
- ✅ Dados sem histórico podem ser apagados pela API, e o padrão de 6 ações continua igual para os outros recursos.
- ⚠️ Cada recurso passa a ter que decidir se expõe `DELETE`, e a revisão do `plan.md` vira a barreira contra o uso indevido.
- ⚠️ Excluir não tem volta: o frontend precisa da confirmação, e os testes cobrem o caso de cancelar.
