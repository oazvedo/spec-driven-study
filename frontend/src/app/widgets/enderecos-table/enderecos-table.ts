import { ChangeDetectionStrategy, Component, computed, inject, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTableModule, NzTableQueryParams } from 'ng-zorro-antd/table';
import { ENDERECO_TIPO_ROTULOS, Endereco, EnderecoApi, descreverEndereco } from '../../entities/endereco';
import { EnderecoExcluir } from '../../features/endereco-excluir';
import { EnderecoToggleAtivo } from '../../features/endereco-toggle-ativo';
import { AtivoTag } from '../../shared/ui';

// Listagem paginada no servidor (RN-09). Não abre o formulário: emite `editar` e a página decide.
@Component({
  selector: 'app-enderecos-table',
  imports: [NzTableModule, NzButtonModule, AtivoTag, EnderecoToggleAtivo, EnderecoExcluir],
  templateUrl: './enderecos-table.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EnderecosTable {
  private readonly api = inject(EnderecoApi);

  readonly editar = output<Endereco>();

  protected readonly page = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly enderecos = rxResource({
    params: () => ({ page: this.page(), pageSize: this.pageSize() }),
    stream: ({ params }) => this.api.listar(params.page, params.pageSize),
  });
  // Em erro o resource não tem valor (ler value() lançaria); a tabela fica vazia e o interceptor mostra a mensagem
  protected readonly itens = computed(() => (this.enderecos.hasValue() ? this.enderecos.value().items : []));
  protected readonly total = computed(() => (this.enderecos.hasValue() ? this.enderecos.value().totalCount : 0));

  protected readonly tipos = ENDERECO_TIPO_ROTULOS;
  protected readonly descrever = descreverEndereco;

  recarregar(): void {
    this.enderecos.reload();
  }

  // Depois de cadastrar: o novo endereço está na página 1 (RN-09, mais recentes primeiro)
  irParaPrimeiraPagina(): void {
    if (this.page() === 1) this.recarregar();
    else this.page.set(1);
  }

  protected mudarPagina({ pageIndex, pageSize }: NzTableQueryParams): void {
    this.page.set(pageIndex);
    this.pageSize.set(pageSize);
  }
}
