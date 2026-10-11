import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { PagedResult } from '../../../shared/api';
import { AtualizarUsuarioRequest, CriarUsuarioRequest, Usuario } from '../model/usuario.model';

// Um método por endpoint de /api/usuarios (ADR 0008)
@Injectable({ providedIn: 'root' })
export class UsuarioApi {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/usuarios';

  listar(page: number, pageSize: number) {
    return this.http.get<PagedResult<Usuario>>(this.url, { params: { page, pageSize } });
  }

  obter(id: string) {
    return this.http.get<Usuario>(`${this.url}/${id}`);
  }

  criar(request: CriarUsuarioRequest, context?: HttpContext) {
    return this.http.post<Usuario>(this.url, request, { context });
  }

  atualizar(id: string, request: AtualizarUsuarioRequest, context?: HttpContext) {
    return this.http.put<Usuario>(`${this.url}/${id}`, request, { context });
  }

  ativar(id: string) {
    return this.http.patch<void>(`${this.url}/${id}/ativar`, null);
  }

  inativar(id: string) {
    return this.http.patch<void>(`${this.url}/${id}/inativar`, null);
  }
}
