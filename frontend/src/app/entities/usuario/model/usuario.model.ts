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

// Mesmos limites de Usuario.NomeMaxLength / EmailMaxLength no backend
export const USUARIO_NOME_MAX = 150;
export const USUARIO_EMAIL_MAX = 254;
