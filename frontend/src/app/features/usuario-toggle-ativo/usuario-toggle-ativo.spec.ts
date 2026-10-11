import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzPopconfirmDirective } from 'ng-zorro-antd/popconfirm';
import { of, throwError } from 'rxjs';
import { Usuario, UsuarioApi } from '../../entities/usuario';
import { UsuarioToggleAtivo } from './usuario-toggle-ativo';

describe('UsuarioToggleAtivo', () => {
  const usuario = (ativo: boolean): Usuario => ({ id: '1', nome: 'Ana', email: 'ana@x.com', ativo, createdAt: '' });
  const api = { ativar: vi.fn(), inativar: vi.fn() };
  let fixture: ComponentFixture<UsuarioToggleAtivo>;
  let alteracoes: number;

  function renderizar(ativo: boolean): void {
    TestBed.configureTestingModule({ providers: [{ provide: UsuarioApi, useValue: api }] });
    fixture = TestBed.createComponent(UsuarioToggleAtivo);
    fixture.componentRef.setInput('usuario', usuario(ativo));
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
  it('usuário ativo: inativa depois da confirmação e avisa que alterou', () => {
    api.inativar.mockReturnValue(of(undefined));
    renderizar(true);

    expect(botao().textContent?.trim()).toBe('Inativar');
    expect(api.inativar).not.toHaveBeenCalled();

    confirmar();

    expect(api.inativar).toHaveBeenCalledWith('1');
    expect(alteracoes).toBe(1);
  });

  // CA-09
  it('usuário inativo: ativa com um clique e avisa que alterou', () => {
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
