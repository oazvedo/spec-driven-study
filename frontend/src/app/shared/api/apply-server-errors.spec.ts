import { FormControl, FormGroup } from '@angular/forms';
import { applyServerErrors } from './apply-server-errors';

describe('applyServerErrors', () => {
  const criarForm = () => new FormGroup({ nome: new FormControl(''), email: new FormControl('') });

  it('marca o controle do campo com a mensagem do servidor', () => {
    const form = criarForm();

    applyServerErrors(form, { errors: { email: ['E-mail inválido.'] } });

    expect(form.controls.email.errors).toEqual({ servidor: 'E-mail inválido.' });
    expect(form.controls.email.touched).toBe(true);
    expect(form.controls.nome.errors).toBeNull();
  });

  it('ignora, sem quebrar, um campo que o formulário não tem', () => {
    const form = criarForm();

    expect(() => applyServerErrors(form, { errors: { campoQueNaoExiste: ['x'] } })).not.toThrow();
    expect(form.valid).toBe(true);
  });
});
