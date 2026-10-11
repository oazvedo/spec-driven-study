import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { UsuarioApi } from './usuario.api';

describe('UsuarioApi', () => {
  let api: UsuarioApi;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(UsuarioApi);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  const request = { nome: 'Ana', email: 'ana@x.com' };
  const casos: { chamar: () => Observable<unknown>; verbo: string; url: string; corpo: unknown }[] = [
    { chamar: () => api.listar(2, 10), verbo: 'GET', url: '/api/usuarios?page=2&pageSize=10', corpo: null },
    { chamar: () => api.obter('1'), verbo: 'GET', url: '/api/usuarios/1', corpo: null },
    { chamar: () => api.criar(request), verbo: 'POST', url: '/api/usuarios', corpo: request },
    { chamar: () => api.atualizar('1', request), verbo: 'PUT', url: '/api/usuarios/1', corpo: request },
    { chamar: () => api.ativar('1'), verbo: 'PATCH', url: '/api/usuarios/1/ativar', corpo: null },
    { chamar: () => api.inativar('1'), verbo: 'PATCH', url: '/api/usuarios/1/inativar', corpo: null },
  ];

  for (const { chamar, verbo, url, corpo } of casos) {
    it(`chama ${verbo} ${url}`, () => {
      chamar().subscribe();

      const req = backend.expectOne((r) => r.urlWithParams === url);
      expect(req.request.method).toBe(verbo);
      expect(req.request.body).toEqual(corpo);
      req.flush(null);
    });
  }

  it('repassa o erro da API para quem chamou', () => {
    let erro: unknown;
    api.obter('inexistente').subscribe({ error: (e) => (erro = e) });

    backend.expectOne('/api/usuarios/inexistente').flush(null, { status: 404, statusText: 'Not Found' });

    expect(erro).toBeDefined();
  });
});
