import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NzMessageService } from 'ng-zorro-antd/message';
import { of } from 'rxjs';
import { Usuario, UsuarioApi } from '../../entities/usuario';
import { UsuariosPage } from './usuarios-page';

describe('UsuariosPage', () => {
  const ana: Usuario = { id: '1', nome: 'Ana', email: 'ana@x.com', ativo: true, createdAt: '' };
  const api = { listar: vi.fn(), criar: vi.fn(), atualizar: vi.fn(), ativar: vi.fn(), inativar: vi.fn() };
  const mensagens = { success: vi.fn(), error: vi.fn() };
  let fixture: ComponentFixture<UsuariosPage>;

  async function atualizar(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    vi.clearAllMocks();
    api.listar.mockReturnValue(of({ items: [ana], page: 1, pageSize: 20, totalCount: 1 }));
    TestBed.configureTestingModule({
      providers: [
        { provide: UsuarioApi, useValue: api },
        { provide: NzMessageService, useValue: mensagens },
      ],
    });
    fixture = TestBed.createComponent(UsuariosPage);
    await atualizar();
  });

  // O nz-modal abre num overlay fora do componente, então modal e campos são buscados no document
  const modal = () => document.querySelector('.ant-modal') as HTMLElement | null;
  const campo = (id: string) => document.querySelector<HTMLInputElement>(`#${id}`)!;

  function digitar(id: string, valor: string): void {
    campo(id).value = valor;
    campo(id).dispatchEvent(new Event('input'));
  }

  // CA-03
  it('Novo usuário abre o modal com o formulário vazio; salvar fecha, avisa e recarrega a tabela', async () => {
    api.criar.mockReturnValue(of({ ...ana, id: '2', nome: 'Bia', email: 'bia@x.com' }));

    fixture.nativeElement.querySelector('.cabecalho button').click();
    await atualizar();
    expect(modal()?.textContent).toContain('Novo usuário');
    expect(campo('usuario-nome').value).toBe('');

    digitar('usuario-nome', 'Bia');
    digitar('usuario-email', 'bia@x.com');
    api.listar.mockClear();
    campo('usuario-nome').form!.dispatchEvent(new Event('submit'));
    await atualizar();

    expect(api.criar).toHaveBeenCalled();
    expect(mensagens.success).toHaveBeenCalledWith('Usuário cadastrado.');
    expect(api.listar).toHaveBeenCalled();
  });

  // CA-07
  it('Editar na tabela abre o modal com os dados do usuário', async () => {
    fixture.nativeElement.querySelector('button[aria-label="Editar Ana"]').click();
    await atualizar();

    expect(modal()?.textContent).toContain('Editar usuário');
    expect(campo('usuario-email').value).toBe('ana@x.com');
  });
});
