import { FormGroup } from '@angular/forms';
import { ProblemDetails } from '../api';

// Leva os erros por campo de um 400 (ProblemDetails.errors) para os controles do formulário.
// O erro fica em `servidor`, para o template exibir no nzErrorTip. Campos que o form não tem são ignorados.
export function applyServerErrors(form: FormGroup, problem: ProblemDetails): void {
  for (const [campo, mensagens] of Object.entries(problem.errors ?? {})) {
    const controle = form.get(campo);
    if (!controle) continue;
    controle.setErrors({ servidor: mensagens.join(' ') });
    controle.markAsTouched();
  }
}
