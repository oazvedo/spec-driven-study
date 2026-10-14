import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzPopconfirmDirective } from 'ng-zorro-antd/popconfirm';
import { of, throwError } from 'rxjs';
import { Endereco, EnderecoApi } from '../../entities/endereco';
import { EnderecoToggleAtivo } from './endereco-toggle-ativo';

describe('EnderecoToggleAtivo', () => {
  const endereco = (ativo: boolean): Endereco => ({
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
    ativo,
    createdAt: '',
  });
  const api = { ativar: vi.fn(), inativar: vi.fn() };
  let fixture: ComponentFixture<EnderecoToggleAtivo>;
  let alteracoes: number;

  function renderizar(ativo: boolean): void {
    TestBed.configureTestingModule({ providers: [{ provide: EnderecoApi, useValue: api }] });
    fixture = TestBed.createComponent(EnderecoToggleAtivo);
    fixture.componentRef.setInput('endereco', endereco(ativo));
    alteracoes = 0;
    fixture.componentInstance.alterado.subscribe(() => alteracoes++);
    fixture.detectChanges();
  }

  const botao = () => fixture.nativeElement.querySelector('button') as HTMLButtonElement;
  // Confirma o nz-popconfirm (o balão abre num overlay; aqui disparamos direto o evento de confirmação)
  const confirmar = () =>
    fixture.debugElement.query(By.directive(NzPopconfirmDirective)).injector.get(NzPopconfirmDirective).nzOnConfirm.emit();

  beforeEach(() => {
    api.ativar.mockReset();
    api.inativar.mockReset();
  });

  // CA-09
  it('endereço ativo: inativa depois da confirmação e avisa que alterou', () => {
    api.inativar.mockReturnValue(of(undefined));
    renderizar(true);

    expect(botao().textContent?.trim()).toBe('Inativar');
    expect(botao().getAttribute('aria-label')).toBe('Inativar Av. Paulista, 1000');
    expect(api.inativar).not.toHaveBeenCalled();

    confirmar();

    expect(api.inativar).toHaveBeenCalledWith('1');
    expect(alteracoes).toBe(1);
  });

  // CA-09
  it('endereço inativo: ativa com um clique e avisa que alterou', () => {
    api.ativar.mockReturnValue(of(undefined));
    renderizar(false);

    expect(botao().textContent?.trim()).toBe('Ativar');
    botao().click();

    expect(api.ativar).toHaveBeenCalledWith('1');
    expect(alteracoes).toBe(1);
  });

  // CA-08 (falha)
  it('erro da API não avisa alteração', () => {
    api.ativar.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    renderizar(false);

    botao().click();

    expect(alteracoes).toBe(0);
  });
});
