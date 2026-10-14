// Espelha os DTOs do backend (ADR 0007)
export interface Endereco {
  id: string;
  usuarioId: string;
  logradouro: string;
  numero: string;
  complemento: string | null;
  bairro: string;
  cidade: string;
  estado: string;
  cep: string;
  tipo: EnderecoTipo;
  ativo: boolean;
  createdAt: string;
}

export interface CriarEnderecoRequest {
  usuarioId: string;
  logradouro: string;
  numero: string;
  complemento: string | null;
  bairro: string;
  cidade: string;
  estado: string;
  cep: string;
  tipo: EnderecoTipo;
}

export type AtualizarEnderecoRequest = CriarEnderecoRequest;

// O enum EnderecoTipo do backend trafega como número no JSON
export type EnderecoTipo = 1 | 2 | 3;

export const ENDERECO_TIPO_ROTULOS: Record<EnderecoTipo, string> = {
  1: 'Comercial',
  2: 'Residencial',
  3: 'Entrega',
};

// Como um endereço aparece em uma linha: na tabela e no aria-label das ações
export const descreverEndereco = (e: Pick<Endereco, 'logradouro' | 'numero'>) => `${e.logradouro}, ${e.numero}`;

// Mesmos limites de Endereco.*MaxLength / EstadoLength no backend
export const ENDERECO_LOGRADOURO_MAX = 100;
export const ENDERECO_NUMERO_MAX = 10;
export const ENDERECO_COMPLEMENTO_MAX = 100;
export const ENDERECO_BAIRRO_MAX = 100;
export const ENDERECO_CIDADE_MAX = 100;
export const ENDERECO_ESTADO_MAX = 2;
export const ENDERECO_CEP_MAX = 9;
