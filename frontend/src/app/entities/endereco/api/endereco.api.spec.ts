import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { CriarEnderecoRequest } from '../model/endereco.model';
import { EnderecoApi } from './endereco.api';

describe('EnderecoApi', () => {
  let api: EnderecoApi;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(EnderecoApi);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  const request: CriarEnderecoRequest = {
    usuarioId: 'u1',
    logradouro: 'Av. Paulista',
    numero: '1000',
    complemento: null,
    bairro: 'Bela Vista',
    cidade: 'São Paulo',
    estado: 'SP',
    cep: '01310-100',
    tipo: 1,
  };
  const casos: { chamar: () => Observable<unknown>; verbo: string; url: string; corpo: unknown }[] = [
    { chamar: () => api.listar(2, 10), verbo: 'GET', url: '/api/enderecos?page=2&pageSize=10', corpo: null },
    { chamar: () => api.obter('1'), verbo: 'GET', url: '/api/enderecos/1', corpo: null },
    { chamar: () => api.criar(request), verbo: 'POST', url: '/api/enderecos', corpo: request },
    { chamar: () => api.atualizar('1', request), verbo: 'PUT', url: '/api/enderecos/1', corpo: request },
    { chamar: () => api.ativar('1'), verbo: 'PATCH', url: '/api/enderecos/1/ativar', corpo: null },
    { chamar: () => api.inativar('1'), verbo: 'PATCH', url: '/api/enderecos/1/inativar', corpo: null },
    { chamar: () => api.excluir('1'), verbo: 'DELETE', url: '/api/enderecos/1', corpo: null },
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
    api.excluir('inexistente').subscribe({ error: (e) => (erro = e) });

    backend.expectOne('/api/enderecos/inexistente').flush(null, { status: 404, statusText: 'Not Found' });

    expect(erro).toBeDefined();
  });
});
