// Campos do PagedResult<T> do backend (ADR 0004) que o frontend usa.
// O backend também devolve totalPages, hasNextPage e hasPreviousPage; o nz-table não precisa deles.
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
