import { HttpContext, HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NzMessageService } from 'ng-zorro-antd/message';
import { of, throwError } from 'rxjs';
import { CriarEnderecoRequest, Endereco, EnderecoApi } from '../../entities/endereco';
import { UsuarioApi } from '../../entities/usuario';
import { STATUS_TRATADOS } from '../../shared/api';
import { EnderecoForm } from './endereco-form';

describe('EnderecoForm', () => {
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
  const existente: Endereco = { ...request, id: '1', ativo: true, createdAt: '2026-10-13T00:00:00Z' };
  const api = { criar: vi.fn(), atualizar: vi.fn() };
  const usuarioApi = { listar: vi.fn() };
  const mensagens = { error: vi.fn() };
  let fixture: ComponentFixture<EnderecoForm>;
  let salvos: Endereco[];

  beforeEach(() => {
    api.criar.mockReset();
    api.atualizar.mockReset();
    mensagens.error.mockReset();
    usuarioApi.listar.mockReset().mockReturnValue(
      of({ items: [{ id: 'u1', nome: 'Ana', email: 'ana@x.com' }], page: 1, pageSize: 100, totalCount: 1 }),
    );
    TestBed.configureTestingModule({
      providers: [
        { provide: EnderecoApi, useValue: api },
        { provide: UsuarioApi, useValue: usuarioApi },
        { provide: NzMessageService, useValue: mensagens },
      ],
    });
    fixture = TestBed.createComponent(EnderecoForm);
    salvos = [];
    fixture.componentInstance.salvo.subscribe((e) => salvos.push(e));
    fixture.detectChanges();
  });

  const el = () => fixture.nativeElement as HTMLElement;

  function digitar(id: string, valor: string): void {
    const input = el().querySelector<HTMLInputElement>(`#endereco-${id}`)!;
    input.value = valor;
    input.dispatchEvent(new Event('input'));
  }

  const digitados = { logradouro: 'Av. Paulista', numero: '1000', bairro: 'Bela Vista', cidade: 'São Paulo', estado: 'SP', cep: '01310-100' };

  // Os inputs são digitados no DOM; os nz-select (usuário e tipo) são escolhidos direto no controle
  function preencher(dados: Partial<typeof digitados> = {}): void {
    for (const [campo, valor] of Object.entries({ ...digitados, ...dados })) digitar(campo, valor);
    fixture.componentInstance['form'].patchValue({ usuarioId: 'u1', tipo: 1 });
  }

  function enviar(): void {
    el().querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  const erroHttp = (status: number, error: object) => throwError(() => new HttpErrorResponse({ status, error }));

  it('carrega os usuários do seletor (no máximo uma página de 100)', () => {
    expect(usuarioApi.listar).toHaveBeenCalledWith(1, 100);
  });

  // CA-03
  it('cria: form válido chama criar, com complemento vazio como null, e emite o endereço salvo', () => {
    api.criar.mockReturnValue(of(existente));

    preencher();
    enviar();

    expect(api.criar).toHaveBeenCalledWith(request, expect.any(HttpContext));
    expect((api.criar.mock.calls[0][1] as HttpContext).get(STATUS_TRATADOS)).toEqual([400, 404]);
    expect(salvos).toEqual([existente]);
  });

  // CA-07
  it('edita: com endereço, preenche o form e chama atualizar', () => {
    api.atualizar.mockReturnValue(of({ ...existente, numero: '2000' }));
    fixture.componentRef.setInput('endereco', existente);
    fixture.detectChanges();

    expect(el().querySelector<HTMLInputElement>('#endereco-logradouro')!.value).toBe('Av. Paulista');

    digitar('numero', '2000');
    enviar();

    expect(api.atualizar).toHaveBeenCalledWith('1', expect.objectContaining({ numero: '2000', tipo: 1 }), expect.any(HttpContext));
    expect(salvos[0].numero).toBe('2000');
  });

  // CA-04 (falha)
  it('form vazio não chama a API e mostra as mensagens dos campos', () => {
    enviar();

    expect(api.criar).not.toHaveBeenCalled();
    expect(el().textContent).toContain('O usuário é obrigatório.');
    expect(el().textContent).toContain('O campo logradouro é obrigatório.');
    expect(el().textContent).toContain('O CEP é obrigatório.');
    expect(el().textContent).toContain('O tipo é obrigatório.');
  });

  // CA-05 (falha)
  it('UF e CEP fora do formato não chamam a API e mostram a mensagem', () => {
    preencher({ estado: 'sp', cep: '1234-567' });
    enviar();

    expect(api.criar).not.toHaveBeenCalled();
    expect(el().textContent).toContain('Informe a UF com 2 letras maiúsculas, por exemplo SP.');
    expect(el().textContent).toContain('CEP inválido. Use 00000-000 ou 00000000.');
  });

  // CA-04 (falha vinda do servidor)
  it('400 do servidor mostra o erro no campo', () => {
    api.criar.mockReturnValue(erroHttp(400, { errors: { cep: ['CEP inválido. Use 00000-000 ou 00000000.'] } }));

    preencher();
    enviar();

    expect(el().textContent).toContain('CEP inválido. Use 00000-000 ou 00000000.');
    expect(salvos).toEqual([]);
  });

  // CA-06 (falha)
  it('404 do servidor (usuário inexistente) mostra a mensagem', () => {
    api.criar.mockReturnValue(erroHttp(404, { detail: 'Usuário u1 não encontrado.' }));

    preencher();
    enviar();

    expect(mensagens.error).toHaveBeenCalledWith('Usuário u1 não encontrado.');
    expect(salvos).toEqual([]);
  });
});
