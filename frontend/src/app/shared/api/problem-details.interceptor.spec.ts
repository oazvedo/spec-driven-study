import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NzMessageService } from 'ng-zorro-antd/message';
import { problemDetailsInterceptor, STATUS_TRATADOS } from './problem-details.interceptor';

describe('problemDetailsInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  const mensagens = { error: vi.fn() };

  beforeEach(() => {
    mensagens.error.mockReset();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([problemDetailsInterceptor])),
        provideHttpClientTesting(),
        { provide: NzMessageService, useValue: mensagens },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  // Faz uma requisição que falha com o status e o corpo dados; devolve o erro que chegou a quem chamou
  function falharCom(status: number, corpo: object | null): unknown {
    let recebido: unknown;
    http.get('/api/x').subscribe({ error: (e) => (recebido = e) });
    backend.expectOne('/api/x').flush(corpo, { status, statusText: 'erro' });
    return recebido;
  }

  it('mostra o detail do ProblemDetails e repassa o erro para quem chamou', () => {
    const erro = falharCom(409, { title: 'Conflito.', detail: 'Já existe um usuário com este e-mail.' });

    expect(mensagens.error).toHaveBeenCalledWith('Já existe um usuário com este e-mail.');
    expect(erro).toBeDefined();
  });

  it('usa uma mensagem genérica quando o corpo não é ProblemDetails', () => {
    falharCom(500, null);

    expect(mensagens.error).toHaveBeenCalledWith('Ocorreu um erro inesperado. Tente novamente.');
  });

  it('não mostra mensagem para um status que quem chamou declarou tratar', () => {
    let erro: unknown;
    const context = new HttpContext().set(STATUS_TRATADOS, [400, 409]);
    http.get('/api/x', { context }).subscribe({ error: (e) => (erro = e) });
    backend.expectOne('/api/x').flush({ detail: 'Já existe.' }, { status: 409, statusText: 'erro' });

    expect(mensagens.error).not.toHaveBeenCalled();
    expect(erro).toBeDefined();
  });

  it('mostra mensagem para 400 quando quem chamou não declarou tratar', () => {
    falharCom(400, { title: 'Dados inválidos.' });

    expect(mensagens.error).toHaveBeenCalledWith('Dados inválidos.');
  });
});
