import { ChangeDetectionStrategy, Component, computed, inject, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTableModule, NzTableQueryParams } from 'ng-zorro-antd/table';
import { Usuario, UsuarioApi } from '../../entities/usuario';
import { UsuarioToggleAtivo } from '../../features/usuario-toggle-ativo';
import { AtivoTag } from '../../shared/ui';

// Listagem paginada no servidor (RN-05). Não abre o formulário: emite `editar` e a página decide.
@Component({
  selector: 'app-usuarios-table',
  imports: [NzTableModule, NzButtonModule, AtivoTag, UsuarioToggleAtivo],
  templateUrl: './usuarios-table.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuariosTable {
  private readonly api = inject(UsuarioApi);

  readonly editar = output<Usuario>();

  protected readonly page = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly usuarios = rxResource({
    params: () => ({ page: this.page(), pageSize: this.pageSize() }),
    stream: ({ params }) => this.api.listar(params.page, params.pageSize),
  });
  // Em erro o resource não tem valor (ler value() lançaria); a tabela fica vazia e o interceptor mostra a mensagem
  protected readonly itens = computed(() => (this.usuarios.hasValue() ? this.usuarios.value().items : []));
  protected readonly total = computed(() => (this.usuarios.hasValue() ? this.usuarios.value().totalCount : 0));

  recarregar(): void {
    this.usuarios.reload();
  }

  // Depois de cadastrar: o novo usuário está na página 1 (RN-05, mais recentes primeiro)
  irParaPrimeiraPagina(): void {
    if (this.page() === 1) this.recarregar();
    else this.page.set(1);
  }

  protected mudarPagina({ pageIndex, pageSize }: NzTableQueryParams): void {
    this.page.set(pageIndex);
    this.pageSize.set(pageSize);
  }
}
