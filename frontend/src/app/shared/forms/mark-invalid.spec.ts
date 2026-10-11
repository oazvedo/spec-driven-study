import { FormControl, FormGroup, Validators } from '@angular/forms';
import { marcarInvalidos } from './mark-invalid';

describe('marcarInvalidos', () => {
  it('marca os controles como dirty e emite statusChanges para o nz-form-control exibir o erro', () => {
    const form = new FormGroup({ nome: new FormControl('', Validators.required) });
    const status: string[] = [];
    form.controls.nome.statusChanges.subscribe((s) => status.push(s));

    marcarInvalidos(form);

    expect(form.controls.nome.dirty).toBe(true);
    expect(status).toEqual(['INVALID']);
  });
});
