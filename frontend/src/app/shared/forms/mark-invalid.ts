import { FormGroup } from '@angular/forms';

// Faz os erros de validação aparecerem num form enviado sem preencher.
// O nz-form-control só exibe o erro quando o controle emite statusChanges; markAllAsTouched não emite.
export function marcarInvalidos(form: FormGroup): void {
  for (const controle of Object.values(form.controls)) {
    controle.markAsDirty();
    controle.updateValueAndValidity({ onlySelf: true });
  }
}
