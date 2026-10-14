import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';

// Botão Ativar/Inativar da exclusão lógica (ADR 0008), igual para toda entidade. Inativar pede confirmação.
// Quem usa faz a chamada à API ao receber `confirmar`.
@Component({
  selector: 'app-ativo-toggle-button',
  imports: [NzButtonModule, NzPopconfirmModule],
  // Com nzCondition verdadeiro (inativo), o popconfirm não abre e confirma direto no clique
  template: `
    <button
      nz-button
      nzType="link"
      nz-popconfirm
      [nzPopconfirmTitle]="confirmacao()"
      nzOkText="Inativar"
      nzCancelText="Cancelar"
      [nzCondition]="!ativo()"
      [nzLoading]="carregando()"
      [attr.aria-label]="acao() + ' ' + rotulo()"
      (nzOnConfirm)="confirmar.emit()"
    >
      {{ acao() }}
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AtivoToggleButton {
  readonly ativo = input.required<boolean>();
  // Identifica o registro para leitores de tela, ex.: "Inativar Ana"
  readonly rotulo = input.required<string>();
  // Pergunta do popconfirm ao inativar, ex.: "Inativar este usuário?"
  readonly confirmacao = input.required<string>();
  readonly carregando = input(false);
  readonly confirmar = output<void>();

  protected readonly acao = computed(() => (this.ativo() ? 'Inativar' : 'Ativar'));
}
