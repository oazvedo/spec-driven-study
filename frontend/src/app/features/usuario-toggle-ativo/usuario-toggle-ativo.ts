import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { finalize } from 'rxjs';
import { Usuario, UsuarioApi } from '../../entities/usuario';

// Ativa ou inativa o usuário (exclusão lógica, ADR 0008). Inativar pede confirmação.
@Component({
  selector: 'app-usuario-toggle-ativo',
  imports: [NzButtonModule, NzPopconfirmModule],
  // Com nzCondition verdadeiro (usuário inativo), o popconfirm não abre e confirma direto no clique
  template: `
    <button
      nz-button
      nzType="link"
      nz-popconfirm
      nzPopconfirmTitle="Inativar este usuário?"
      nzOkText="Inativar"
      nzCancelText="Cancelar"
      [nzCondition]="!usuario().ativo"
      [nzLoading]="alterando()"
      [attr.aria-label]="acao() + ' ' + usuario().nome"
      (nzOnConfirm)="alterar()"
    >
      {{ acao() }}
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioToggleAtivo {
  private readonly api = inject(UsuarioApi);

  readonly usuario = input.required<Usuario>();
  readonly alterado = output<void>();

  protected readonly alterando = signal(false);
  protected readonly acao = computed(() => (this.usuario().ativo ? 'Inativar' : 'Ativar'));

  protected alterar(): void {
    const { id, ativo } = this.usuario();
    this.alterando.set(true);
    (ativo ? this.api.inativar(id) : this.api.ativar(id))
      .pipe(finalize(() => this.alterando.set(false)))
      .subscribe({
        next: () => this.alterado.emit(),
        error: () => undefined, // o problemDetailsInterceptor já mostrou a mensagem
      });
  }
}
