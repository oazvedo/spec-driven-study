import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { Endereco, EnderecoApi, descreverEndereco } from '../../entities/endereco';
import { AtivoToggleButton } from '../../shared/ui';

// Ativa ou inativa o endereço (exclusão lógica, ADR 0008). O botão e a confirmação vêm de shared/ui.
@Component({
  selector: 'app-endereco-toggle-ativo',
  imports: [AtivoToggleButton],
  template: `
    <app-ativo-toggle-button
      [ativo]="endereco().ativo"
      [rotulo]="descricao()"
      confirmacao="Inativar este endereço?"
      [carregando]="alterando()"
      (confirmar)="alterar()"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EnderecoToggleAtivo {
  private readonly api = inject(EnderecoApi);

  readonly endereco = input.required<Endereco>();
  readonly alterado = output<void>();

  protected readonly alterando = signal(false);
  protected readonly descricao = computed(() => descreverEndereco(this.endereco()));

  protected alterar(): void {
    const { id, ativo } = this.endereco();
    this.alterando.set(true);
    (ativo ? this.api.inativar(id) : this.api.ativar(id))
      .pipe(finalize(() => this.alterando.set(false)))
      .subscribe({
        next: () => this.alterado.emit(),
        error: () => undefined, // o problemDetailsInterceptor já mostrou a mensagem
      });
  }
}
