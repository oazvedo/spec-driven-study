// Espelha os DTOs do backend (ADR 0007)
export interface Usuario {
  id: string;
  nome: string;
  email: string;
  ativo: boolean;
  createdAt: string;
}

export interface CriarUsuarioRequest {
  nome: string;
  email: string;
}

export type AtualizarUsuarioRequest = CriarUsuarioRequest;
