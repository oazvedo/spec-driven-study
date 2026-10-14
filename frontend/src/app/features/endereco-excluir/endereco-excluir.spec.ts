import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzPopconfirmDirective } from 'ng-zorro-antd/popconfirm';
import { of, throwError } from 'rxjs';
import { Endereco, EnderecoApi } from '../../entities/endereco';
import { EnderecoExcluir } from './endereco-excluir';

describe('EnderecoExcluir', () => {
  const endereco: Endereco = {
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
  const api = { excluir: vi.fn() };
  let fixture: ComponentFixture<EnderecoExcluir>;
  let exclusoes: number;

  beforeEach(() => {
    api.excluir.mockReset();
    TestBed.configureTestingModule({ providers: [{ provide: EnderecoApi, useValue: api }] });
    fixture = TestBed.createComponent(EnderecoExcluir);
    fixture.componentRef.setInput('endereco', endereco);
    exclusoes = 0;
    fixture.componentInstance.excluido.subscribe(() => exclusoes++);
    fixture.detectChanges();
  });

  const botao = () => fixture.nativeElement.querySelector('button') as HTMLButtonElement;
  // O balão do nz-popconfirm abre num overlay; aqui disparamos direto os eventos de confirmar e cancelar
  const popconfirm = () => fixture.debugElement.query(By.directive(NzPopconfirmDirective)).injector.get(NzPopconfirmDirective);

  // CA-10
  it('confirmar a exclusão chama excluir e avisa que excluiu', () => {
    api.excluir.mockReturnValue(of(undefined));

    expect(botao().getAttribute('aria-label')).toBe('Excluir Av. Paulista, 1000');
    popconfirm().nzOnConfirm.emit();

    expect(api.excluir).toHaveBeenCalledWith('1');
    expect(exclusoes).toBe(1);
  });

  // CA-11 (falha)
  it('clicar e cancelar a confirmação não chama a API', () => {
    botao().click();
    popconfirm().nzOnCancel.emit();

    expect(api.excluir).not.toHaveBeenCalled();
    expect(exclusoes).toBe(0);
  });

  // CA-08 (falha)
  it('erro da API não avisa exclusão', () => {
    api.excluir.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));

    popconfirm().nzOnConfirm.emit();

    expect(exclusoes).toBe(0);
  });
});
