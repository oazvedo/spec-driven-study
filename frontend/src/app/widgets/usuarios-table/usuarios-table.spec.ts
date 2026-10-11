import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzTableComponent, NzTableQueryParams } from 'ng-zorro-antd/table';
import { of, throwError } from 'rxjs';
import { Usuario, UsuarioApi } from '../../entities/usuario';
import { UsuarioToggleAtivo } from '../../features/usuario-toggle-ativo';
import { PagedResult } from '../../shared/api';
import { UsuariosTable } from './usuarios-table';

describe('UsuariosTable', () => {
  const ana: Usuario = { id: '1', nome: 'Ana', email: 'ana@x.com', ativo: true, createdAt: '' };
  const pagina = (items: Usuario[]): PagedResult<Usuario> => ({ items, page: 1, pageSize: 20, totalCount: items.length });
  const api = { listar: vi.fn(), ativar: vi.fn(), inativar: vi.fn() };
  let fixture: ComponentFixture<UsuariosTable>;

  async function renderizar(): Promise<void> {
    TestBed.configureTestingModule({ providers: [{ provide: UsuarioApi, useValue: api }] });
    fixture = TestBed.createComponent(UsuariosTable);
    fixture.detectChanges();
    await fixture.whenStable();
  }

  const linhas = () => fixture.nativeElement.querySelectorAll('tbody tr.ant-table-row') as NodeListOf<HTMLElement>;

  beforeEach(() => {
    api.listar.mockReset();
    api.listar.mockReturnValue(of(pagina([ana])));
  });

  // CA-01
  it('carrega a primeira página com 20 itens e mostra os usuários', async () => {
    await renderizar();

    expect(api.listar).toHaveBeenCalledWith(1, 20);
    expect(linhas().length).toBe(1);
    expect(linhas()[0].textContent).toContain('ana@x.com');
    expect(linhas()[0].textContent).toContain('Ativo');
  });

  // CA-01
  it('trocar de página busca a página nova no servidor', async () => {
    await renderizar();

    const tabela = fixture.debugElement.query(By.directive(NzTableComponent)).componentInstance as NzTableComponent<Usuario>;
    tabela.nzQueryParams.emit({ pageIndex: 2, pageSize: 50, sort: [], filter: [] } as NzTableQueryParams);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.listar).toHaveBeenLastCalledWith(2, 50);
  });

  it('clicar em Editar emite o usuário da linha', async () => {
    await renderizar();
    const editados: Usuario[] = [];
    fixture.componentInstance.editar.subscribe((u) => editados.push(u));

    (fixture.nativeElement.querySelector('button[aria-label="Editar Ana"]') as HTMLButtonElement).click();

    expect(editados).toEqual([ana]);
  });

  // CA-09: depois de ativar/inativar, a lista é buscada de novo para mostrar o status atualizado
  it('recarrega a lista quando um usuário é ativado ou inativado', async () => {
    await renderizar();
    api.listar.mockClear();

    fixture.debugElement.query(By.directive(UsuarioToggleAtivo)).componentInstance.alterado.emit();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(api.listar).toHaveBeenCalledWith(1, 20);
  });

  // Falha: erro da API deixa a tabela vazia, sem quebrar (a mensagem fica com o interceptor)
  it('erro ao listar deixa a tabela vazia', async () => {
    api.listar.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));

    await renderizar();

    expect(linhas().length).toBe(0);
  });
});
