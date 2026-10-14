import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { PagedResult } from '../../../shared/api';
import { AtualizarEnderecoRequest, CriarEnderecoRequest, Endereco } from '../model/endereco.model';

// Um método por endpoint de /api/enderecos (ADR 0008)
@Injectable({ providedIn: 'root' })
export class EnderecoApi {
    private readonly http = inject(HttpClient);
    private readonly url = '/api/enderecos';

    listar(page: number, pageSize: number) {
        return this.http.get<PagedResult<Endereco>>(this.url, { params: { page, pageSize } });
    }

    obter(id: string) {
        return this.http.get<Endereco>(`${this.url}/${id}`);
    }

    criar(request: CriarEnderecoRequest, context?: HttpContext) {
        return this.http.post<Endereco>(this.url, request, { context });
    }

    atualizar(id: string, request: AtualizarEnderecoRequest, context?: HttpContext) {
        return this.http.put<Endereco>(`${this.url}/${id}`, request, { context });
    }

    ativar(id: string) {
        return this.http.patch<void>(`${this.url}/${id}/ativar`, null);
    }

    inativar(id: string) {
        return this.http.patch<void>(`${this.url}/${id}/inativar`, null);
    }

    excluir(id: string) {
        return this.http.delete<void>(`${this.url}/${id}`);
    }
}