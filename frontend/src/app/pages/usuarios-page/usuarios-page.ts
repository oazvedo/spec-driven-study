import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { Usuario } from '../../entities/usuario';
import { UsuarioForm } from '../../features/usuario-form';
import { UsuariosTable } from '../../widgets/usuarios-table';

// Tela de usuários: a tabela e o formulário (criar/editar) num modal
@Component({
  selector: 'app-usuarios-page',
  imports: [NzButtonModule, NzModalModule, UsuariosTable, UsuarioForm],
  templateUrl: './usuarios-page.html',
  styleUrl: './usuarios-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuariosPage {
  private readonly mensagens = inject(NzMessageService);
  private readonly tabela = viewChild.required(UsuariosTable);

  protected readonly modalAberto = signal(false);
  protected readonly emEdicao = signal<Usuario | null>(null);

  protected abrir(usuario: Usuario | null): void {
    this.emEdicao.set(usuario);
    this.modalAberto.set(true);
  }

  protected fechar(): void {
    this.modalAberto.set(false);
  }

  protected aoSalvar(): void {
    const editando = this.emEdicao() !== null;
    this.mensagens.success(editando ? 'Usuário atualizado.' : 'Usuário cadastrado.');
    this.fechar();
    // Quem edita continua na página em que estava; quem cadastra vai ver o novo usuário na página 1
    if (editando) this.tabela().recarregar();
    else this.tabela().irParaPrimeiraPagina();
  }
}
