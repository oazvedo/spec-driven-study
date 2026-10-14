import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { Usuario, UsuarioApi } from '../../entities/usuario';
import { AtivoToggleButton } from '../../shared/ui';

// Ativa ou inativa o usuário (exclusão lógica, ADR 0008). O botão e a confirmação vêm de shared/ui.
@Component({
  selector: 'app-usuario-toggle-ativo',
  imports: [AtivoToggleButton],
  template: `
    <app-ativo-toggle-button
      [ativo]="usuario().ativo"
      [rotulo]="usuario().nome"
      confirmacao="Inativar este usuário?"
      [carregando]="alterando()"
      (confirmar)="alterar()"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioToggleAtivo {
  private readonly api = inject(UsuarioApi);

  readonly usuario = input.required<Usuario>();
  readonly alterado = output<void>();

  protected readonly alterando = signal(false);

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
