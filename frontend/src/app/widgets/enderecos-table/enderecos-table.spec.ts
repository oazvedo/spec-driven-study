import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzTableComponent, NzTableQueryParams } from 'ng-zorro-antd/table';
import { of, throwError } from 'rxjs';
import { Endereco, EnderecoApi } from '../../entities/endereco';
import { EnderecoExcluir } from '../../features/endereco-excluir';
import { EnderecoToggleAtivo } from '../../features/endereco-toggle-ativo';
import { PagedResult } from '../../shared/api';
import { EnderecosTable } from './enderecos-table';

describe('EnderecosTable', () => {
  const paulista: Endereco = {
    id: '1',
    usuarioId: 'u1',
    usuarioNome: 'Ana',
    logradouro: 'Av. Paulista',
    numero: '1000',
    complemento: null,
    bairro: 'Bela Vista',
    cidade: 'São Paulo',
    estado: 'SP',
    cep: '01310-100',
    tipo: 1,
    ativo: true,
    createdAt: '',
  };
  const pagina = (items: Endereco[]): PagedResult<Endereco> => ({ items, page: 1, pageSize: 20, totalCount: items.length });
  const api = { listar: vi.fn(), ativar: vi.fn(), inativar: vi.fn(), excluir: vi.fn() };
  let fixture: ComponentFixture<EnderecosTable>;

  async function renderizar(): Promise<void> {
    TestBed.configureTestingModule({ providers: [{ provide: EnderecoApi, useValue: api }] });
    fixture = TestBed.createComponent(EnderecosTable);
    await estabilizar();
  }

  async function irParaPagina(pageIndex: number, pageSize: number): Promise<void> {
    const tabela = fixture.debugElement.query(By.directive(NzTableComponent)).componentInstance as NzTableComponent<Endereco>;
    tabela.nzQueryParams.emit({ pageIndex, pageSize, sort: [], filter: [] } as NzTableQueryParams);
    await estabilizar();
  }

  async function estabilizar(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
  }

  const linhas = () => fixture.nativeElement.querySelectorAll('tbody tr.ant-table-row') as NodeListOf<HTMLElement>;

  beforeEach(() => {
    api.listar.mockReset().mockReturnValue(of(pagina([paulista])));
  });

  // CA-01
  it('carrega a primeira página com 20 itens e mostra os endereços', async () => {
    await renderizar();

    expect(api.listar).toHaveBeenCalledWith(1, 20);
    expect(linhas().length).toBe(1);
    const texto = linhas()[0].textContent;
    expect(texto).toContain('Av. Paulista, 1000');
    expect(texto).toContain('São Paulo/SP');
    expect(texto).toContain('Comercial');
    expect(texto).toContain('Ativo');
  });

  // T08b: o nome vem da API; a tabela não busca usuários
  it('a coluna usuário mostra o usuarioNome devolvido pela API', async () => {
    await renderizar();

    expect(linhas()[0].querySelector('td')!.textContent?.trim()).toBe('Ana');
  });

  // CA-01
  it('trocar de página busca a página nova no servidor', async () => {
    await renderizar();

    await irParaPagina(2, 50);

    expect(api.listar).toHaveBeenLastCalledWith(2, 50);
  });

  it('irParaPrimeiraPagina volta para a página 1 mantendo o tamanho da página', async () => {
    await renderizar();
    await irParaPagina(3, 50);

    fixture.componentInstance.irParaPrimeiraPagina();
    await estabilizar();

    expect(api.listar).toHaveBeenLastCalledWith(1, 50);
  });

  it('clicar em Editar emite o endereço da linha', async () => {
    await renderizar();
    const editados: Endereco[] = [];
    fixture.componentInstance.editar.subscribe((e) => editados.push(e));

    (fixture.nativeElement.querySelector('button[aria-label="Editar Av. Paulista, 1000"]') as HTMLButtonElement).click();

    expect(editados).toEqual([paulista]);
  });

  // CA-09 e CA-10: depois de ativar/inativar ou excluir, a lista é buscada de novo
  it('recarrega a lista quando um endereço é ativado, inativado ou excluído', async () => {
    await renderizar();
    api.listar.mockClear();

    fixture.debugElement.query(By.directive(EnderecoToggleAtivo)).componentInstance.alterado.emit();
    await estabilizar();
    fixture.debugElement.query(By.directive(EnderecoExcluir)).componentInstance.excluido.emit();
    await estabilizar();

    expect(api.listar).toHaveBeenCalledTimes(2);
    expect(api.listar).toHaveBeenCalledWith(1, 20);
  });

  // Falha: erro da API deixa a tabela vazia, sem quebrar (a mensagem fica com o interceptor)
  it('erro ao listar deixa a tabela vazia', async () => {
    api.listar.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));

    await renderizar();

    expect(linhas().length).toBe(0);
  });
});
