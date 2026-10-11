import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { tap } from 'rxjs';
import { ProblemDetails } from './problem-details';

// Mostra um nz-message para os erros da API. O 400 fica de fora: os erros por campo são exibidos
// no próprio formulário (applyServerErrors). O erro segue adiante para quem fez a chamada.
export const problemDetailsInterceptor: HttpInterceptorFn = (req, next) => {
  const mensagens = inject(NzMessageService);

  return next(req).pipe(
    tap({
      error: (erro: unknown) => {
        if (erro instanceof HttpErrorResponse && erro.status !== 400) {
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
