import { HttpContext, HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, effect, inject, input, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { finalize } from 'rxjs';
import {
  ENDERECO_BAIRRO_MAX,
  ENDERECO_CIDADE_MAX,
  ENDERECO_COMPLEMENTO_MAX,
  ENDERECO_ESTADO_MAX,
  ENDERECO_LOGRADOURO_MAX,
  ENDERECO_NUMERO_MAX,
  ENDERECO_TIPO_ROTULOS,
  Endereco,
  EnderecoApi,
  EnderecoTipo,
} from '../../entities/endereco';
import { UsuarioApi } from '../../entities/usuario';
import { PAGE_SIZE_MAX, ProblemDetails, STATUS_TRATADOS } from '../../shared/api';
import { applyServerErrors, marcarInvalidos } from '../../shared/forms';

// Este formulário mostra os erros 400 (por campo) e 404 (usuário inexistente) ele mesmo
const CONTEXTO = new HttpContext().set(STATUS_TRATADOS, [400, 404]);

type Campo = 'usuarioId' | 'logradouro' | 'numero' | 'complemento' | 'bairro' | 'cidade' | 'estado' | 'cep' | 'tipo';

const OBRIGATORIO: Partial<Record<Campo, string>> = {
  usuarioId: 'O usuário é obrigatório.',
  logradouro: 'O campo logradouro é obrigatório.',
  numero: 'O campo número é obrigatório.',
  bairro: 'O campo bairro é obrigatório.',
  cidade: 'O campo cidade é obrigatório.',
  estado: 'O estado é obrigatório.',
  cep: 'O CEP é obrigatório.',
  tipo: 'O tipo é obrigatório.',
};

// Mesmo formato do validator do backend (RN-04 e RN-05)
const FORMATO: Partial<Record<Campo, string>> = {
  estado: 'Informe a UF com 2 letras maiúsculas, por exemplo SP.',
  cep: 'CEP inválido. Use 00000-000 ou 00000000.',
};

const texto = (...validators: ValidatorFn[]) => new FormControl('', { nonNullable: true, validators });

// Cria (sem endereço) ou edita (com endereço) e emite o endereço salvo
@Component({
  selector: 'app-endereco-form',
  imports: [ReactiveFormsModule, NzFormModule, NzInputModule, NzSelectModule, NzButtonModule],
  templateUrl: './endereco-form.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EnderecoForm {
  private readonly api = inject(EnderecoApi);
  private readonly usuarioApi = inject(UsuarioApi);
  private readonly mensagens = inject(NzMessageService);

  readonly endereco = input<Endereco | null>(null);
  readonly salvo = output<Endereco>();

  // No máximo uma página de usuários (risco aceito no plan.md)
  protected readonly usuarios = rxResource({ stream: () => this.usuarioApi.listar(1, PAGE_SIZE_MAX) });
  protected readonly estadoMax = ENDERECO_ESTADO_MAX;
  protected readonly tipos = Object.entries(ENDERECO_TIPO_ROTULOS).map(([valor, rotulo]) => ({
    valor: Number(valor) as EnderecoTipo,
    rotulo,
  }));

  protected readonly salvando = signal(false);
  protected readonly form = new FormGroup({
    usuarioId: texto(Validators.required),
    logradouro: texto(Validators.required, Validators.maxLength(ENDERECO_LOGRADOURO_MAX)),
    numero: texto(Validators.required, Validators.maxLength(ENDERECO_NUMERO_MAX)),
    complemento: texto(Validators.maxLength(ENDERECO_COMPLEMENTO_MAX)),
    bairro: texto(Validators.required, Validators.maxLength(ENDERECO_BAIRRO_MAX)),
    cidade: texto(Validators.required, Validators.maxLength(ENDERECO_CIDADE_MAX)),
    estado: texto(Validators.required, Validators.pattern(new RegExp(`^[A-Z]{${ENDERECO_ESTADO_MAX}}$`))),
    cep: texto(Validators.required, Validators.pattern(/^\d{5}-?\d{3}$/)),
    tipo: new FormControl<EnderecoTipo | null>(null, { validators: [Validators.required] }),
  });

  constructor() {
    // Sem endereço, os controles voltam ao padrão; campos que o form não tem (id, ativo) são ignorados
    effect(() => {
      const endereco = this.endereco();
      this.form.reset(endereco ? { ...endereco, complemento: endereco.complemento ?? '' } : undefined);
    });
  }

  protected salvar(): void {
    // O nzLoading só bloqueia o clique; um Enter no campo enviaria de novo
    if (this.salvando()) return;
    if (this.form.invalid) {
      marcarInvalidos(this.form);
      return;
    }

    const endereco = this.endereco();
    const valores = this.form.getRawValue();
    // O tipo já passou pelo required; o complemento vazio vai como null (opcional, RN-02)
    const request = { ...valores, complemento: valores.complemento || null, tipo: valores.tipo! };
    const chamada = endereco
      ? this.api.atualizar(endereco.id, request, CONTEXTO)
      : this.api.criar(request, CONTEXTO);

    this.salvando.set(true);
    chamada.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (salvo) => this.salvo.emit(salvo),
      error: (erro: HttpErrorResponse) => this.mostrarErroDoServidor(erro),
    });
  }

  private mostrarErroDoServidor(erro: HttpErrorResponse): void {
    const problem = (erro.error ?? {}) as ProblemDetails;
    if (erro.status === 400) {
      // O form declarou tratar o 400: o que não cabe em nenhum campo ele mesmo avisa
      const semCampo = applyServerErrors(this.form, problem);
      if (semCampo.length) this.mensagens.error(semCampo.join(' '));
    } else if (erro.status === 404) {
      // Usuário removido do seletor por outra pessoa, ou o próprio endereço já excluído (CA-06 e CA-08)
      this.mensagens.error(problem.detail ?? 'Registro não encontrado.');
    }
  }

  protected mensagemDeErro(campo: Campo): string {
    const erros = this.form.controls[campo].errors;
    if (!erros) return '';
    if (erros['servidor']) return erros['servidor'];
    if (erros['required']) return OBRIGATORIO[campo] ?? '';
    if (erros['pattern']) return FORMATO[campo] ?? '';
    if (erros['maxlength']) return `Use no máximo ${erros['maxlength'].requiredLength} caracteres.`;
    return '';
  }
}
