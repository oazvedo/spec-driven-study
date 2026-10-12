import { HttpContext, HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, effect, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzMessageService } from 'ng-zorro-antd/message';
import { finalize } from 'rxjs';
import { USUARIO_EMAIL_MAX, USUARIO_NOME_MAX, Usuario, UsuarioApi } from '../../entities/usuario';
import { ProblemDetails, STATUS_TRATADOS } from '../../shared/api';
import { applyServerErrors, marcarInvalidos } from '../../shared/forms';

// Este formulário mostra os erros 400 (por campo) e 409 (e-mail duplicado) no próprio campo
const CONTEXTO = new HttpContext().set(STATUS_TRATADOS, [400, 409]);

const OBRIGATORIO = { nome: 'O nome é obrigatório.', email: 'O e-mail é obrigatório.' };

// Cria (sem usuário) ou edita (com usuário) e emite o usuário salvo
@Component({
  selector: 'app-usuario-form',
  imports: [ReactiveFormsModule, NzFormModule, NzInputModule, NzButtonModule],
  templateUrl: './usuario-form.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioForm {
  private readonly api = inject(UsuarioApi);
  private readonly mensagens = inject(NzMessageService);

  readonly usuario = input<Usuario | null>(null);
  readonly salvo = output<Usuario>();

  protected readonly salvando = signal(false);
  protected readonly form = new FormGroup({
    nome: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(USUARIO_NOME_MAX)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(USUARIO_EMAIL_MAX)],
    }),
  });

  constructor() {
    // Sem usuário, os controles nonNullable voltam para ''; campos que o form não tem (id, ativo) são ignorados
    effect(() => this.form.reset(this.usuario() ?? undefined));
  }

  protected salvar(): void {
    // O nzLoading só bloqueia o clique; um Enter no campo enviaria de novo
    if (this.salvando()) return;
    if (this.form.invalid) {
      marcarInvalidos(this.form);
      return;
    }

    const usuario = this.usuario();
    const request = this.form.getRawValue();
    const chamada = usuario
      ? this.api.atualizar(usuario.id, request, CONTEXTO)
      : this.api.criar(request, CONTEXTO);

    this.salvando.set(true);
    chamada.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (salvo) => this.salvo.emit(salvo),
      error: (erro: HttpErrorResponse) => this.mostrarErroDoServidor(erro),
    });
  }

  private mostrarErroDoServidor(erro: HttpErrorResponse): void {
    const problem = (erro.error ?? {}) as ProblemDetails;
    if (erro.status === 400) {
      // O form declarou tratar o 400: o que não cabe em nenhum campo ele mesmo avisa
      const semCampo = applyServerErrors(this.form, problem);
      if (semCampo.length) this.mensagens.error(semCampo.join(' '));
    } else if (erro.status === 409) {
      // O 409 de usuário é sempre e-mail duplicado (RN-02)
      applyServerErrors(this.form, { errors: { email: [problem.detail ?? 'Este e-mail já está em uso.'] } });
    }
  }

  protected mensagemDeErro(campo: 'nome' | 'email'): string {
    const erros = this.form.controls[campo].errors;
    if (!erros) return '';
    if (erros['servidor']) return erros['servidor'];
    if (erros['required']) return OBRIGATORIO[campo];
    if (erros['email']) return 'E-mail inválido.';
    if (erros['maxlength']) return `Use no máximo ${erros['maxlength'].requiredLength} caracteres.`;
    return '';
  }
}
