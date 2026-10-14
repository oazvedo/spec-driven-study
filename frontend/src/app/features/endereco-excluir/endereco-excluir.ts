import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { finalize } from 'rxjs';
import { Endereco, EnderecoApi, descreverEndereco } from '../../entities/endereco';

// Exclui o endereço de vez (ADR 0016). Sempre pede confirmação, porque não tem volta (CA-11).
@Component({
  selector: 'app-endereco-excluir',
  imports: [NzButtonModule, NzPopconfirmModule],
  template: `
    <button
      nz-button
      nzType="link"
      nzDanger
      nz-popconfirm
      nzPopconfirmTitle="Excluir este endereço? Não dá para desfazer."
      nzOkText="Excluir"
      nzOkDanger
      nzCancelText="Cancelar"
      [nzLoading]="excluindo()"
      [attr.aria-label]="'Excluir ' + descricao()"
      (nzOnConfirm)="excluir()"
    >
      Excluir
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EnderecoExcluir {
  private readonly api = inject(EnderecoApi);

  readonly endereco = input.required<Endereco>();
  readonly excluido = output<void>();

  protected readonly excluindo = signal(false);
  protected readonly descricao = computed(() => descreverEndereco(this.endereco()));

  protected excluir(): void {
    this.excluindo.set(true);
    this.api
      .excluir(this.endereco().id)
      .pipe(finalize(() => this.excluindo.set(false)))
      .subscribe({
        next: () => this.excluido.emit(),
        error: () => undefined, // o problemDetailsInterceptor já mostrou a mensagem
      });
  }
}
