// Formato de erro da API (RFC 9457, ADR 0006). `errors` só vem no 400, com os campos em camelCase.
export interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
