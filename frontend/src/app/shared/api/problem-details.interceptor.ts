import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { tap } from 'rxjs';
import { ProblemDetails } from './problem-details';

// Status HTTP que quem fez a chamada vai tratar na própria tela; para eles o interceptor não mostra nz-message.
// Padrão: 400, cujos erros por campo vão para o formulário (applyServerErrors).
// Ex.: o formulário de usuário declara [400, 409] para mostrar o e-mail duplicado no próprio campo.
export const STATUS_TRATADOS = new HttpContextToken<readonly number[]>(() => [400]);

// Mostra um nz-message para os erros da API que ninguém declarou tratar. O erro segue adiante para quem chamou.
export const problemDetailsInterceptor: HttpInterceptorFn = (req, next) => {
  const mensagens = inject(NzMessageService);

  return next(req).pipe(
    tap({
      error: (erro: unknown) => {
        if (erro instanceof HttpErrorResponse && !req.context.get(STATUS_TRATADOS).includes(erro.status)) {
          mensagens.error(mensagemDeErro(erro));
        }
      },
    }),
  );
};

function mensagemDeErro(erro: HttpErrorResponse): string {
  if (erro.status === 0) {
    return 'Não foi possível conectar ao servidor.';
  }
  const problem = erro.error as ProblemDetails | null;
  return problem?.detail ?? problem?.title ?? 'Ocorreu um erro inesperado. Tente novamente.';
}
