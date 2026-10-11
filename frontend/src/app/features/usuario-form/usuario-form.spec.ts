import { HttpContext, HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Usuario, UsuarioApi } from '../../entities/usuario';
import { STATUS_TRATADOS } from '../../shared/api';
import { UsuarioForm } from './usuario-form';

describe('UsuarioForm', () => {
  const existente: Usuario = { id: '1', nome: 'Ana', email: 'ana@x.com', ativo: true, createdAt: '2026-10-10T00:00:00Z' };
  const api = { criar: vi.fn(), atualizar: vi.fn() };
  let fixture: ComponentFixture<UsuarioForm>;
  let salvos: Usuario[];

  beforeEach(() => {
    api.criar.mockReset();
    api.atualizar.mockReset();
    TestBed.configureTestingModule({ providers: [{ provide: UsuarioApi, useValue: api }] });
    fixture = TestBed.createComponent(UsuarioForm);
    salvos = [];
    fixture.componentInstance.salvo.subscribe((u) => salvos.push(u));
    fixture.detectChanges();
  });

  const el = () => fixture.nativeElement as HTMLElement;

  function digitar(id: string, valor: string): void {
    const input = el().querySelector<HTMLInputElement>(`#${id}`)!;
    input.value = valor;
    input.dispatchEvent(new Event('input'));
  }

  function preencher(nome: string, email: string): void {
    digitar('usuario-nome', nome);
    digitar('usuario-email', email);
  }

  function enviar(): void {
    el().querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  const erroHttp = (status: number, error: object) => throwError(() => new HttpErrorResponse({ status, error }));

  // CA-03
  it('cria: form válido chama criar e emite o usuário salvo', () => {
    api.criar.mockReturnValue(of(existente));

    preencher('Ana', 'ana@x.com');
    enviar();

    expect(api.criar).toHaveBeenCalledWith({ nome: 'Ana', email: 'ana@x.com' }, expect.any(HttpContext));
    expect((api.criar.mock.calls[0][1] as HttpContext).get(STATUS_TRATADOS)).toEqual([400, 409]);
    expect(salvos).toEqual([existente]);
  });

  // CA-07
  it('edita: com usuário, preenche o form e chama atualizar', () => {
    api.atualizar.mockReturnValue(of({ ...existente, nome: 'Ana Maria' }));
    fixture.componentRef.setInput('usuario', existente);
    fixture.detectChanges();

    expect(el().querySelector<HTMLInputElement>('#usuario-nome')!.value).toBe('Ana');

    preencher('Ana Maria', 'ana@x.com');
    enviar();

    expect(api.atualizar).toHaveBeenCalledWith('1', { nome: 'Ana Maria', email: 'ana@x.com' }, expect.any(HttpContext));
    expect(salvos[0].nome).toBe('Ana Maria');
  });

  // CA-04 (falha)
  it('form vazio não chama a API e mostra as mensagens dos campos', () => {
    enviar();

    expect(api.criar).not.toHaveBeenCalled();
    expect(el().textContent).toContain('O nome é obrigatório.');
    expect(el().textContent).toContain('O e-mail é obrigatório.');
  });

  // CA-05 (falha)
  it('e-mail inválido não chama a API', () => {
    preencher('Ana', 'nao-e-email');
    enviar();

    expect(api.criar).not.toHaveBeenCalled();
    expect(el().textContent).toContain('E-mail inválido.');
  });

  // CA-04 (falha vinda do servidor)
  it('400 do servidor mostra o erro no campo', () => {
    api.criar.mockReturnValue(erroHttp(400, { errors: { nome: ['O nome é obrigatório.'] } }));

    preencher('   ', 'ana@x.com');
    enviar();

    expect(el().textContent).toContain('O nome é obrigatório.');
    expect(salvos).toEqual([]);
  });

  // CA-06 (falha)
  it('409 do servidor mostra o e-mail duplicado no campo e-mail', () => {
    api.criar.mockReturnValue(erroHttp(409, { detail: 'Já existe um usuário com este e-mail.' }));

    preencher('Ana', 'ana@x.com');
    enviar();

    expect(el().textContent).toContain('Já existe um usuário com este e-mail.');
    expect(salvos).toEqual([]);
  });
});
