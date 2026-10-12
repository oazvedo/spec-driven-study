import { FormGroup } from '@angular/forms';
import { ProblemDetails } from '../api';

// Leva os erros por campo de um 400 (ProblemDetails.errors) para os controles do formulário.
// O erro fica em `servidor`, para o template exibir no nzErrorTip.
// Retorna as mensagens que não couberam em nenhum campo (ex.: JSON malformado), para quem chamou exibir de outro jeito.
export function applyServerErrors(form: FormGroup, problem: ProblemDetails): string[] {
  const semCampo: string[] = [];
  for (const [campo, mensagens] of Object.entries(problem.errors ?? {})) {
    const controle = form.get(campo);
    if (!controle) {
      semCampo.push(...mensagens);
      continue;
    }
    controle.setErrors({ servidor: mensagens.join(' ') });
    controle.markAsTouched();
  }
  return problem.errors ? semCampo : [problem.title ?? 'Dados inválidos.'];
}
