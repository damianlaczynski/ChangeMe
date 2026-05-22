import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';
import { PaginationResult } from '@shared/data/models/pagination-result.model';

export function createEmptyPaginationResult<T>(
  params: Partial<Pick<PaginationParameters, 'pageNumber' | 'pageSize'>> = {}
): PaginationResult<T> {
  return {
    items: [],
    currentPage: params.pageNumber ?? 1,
    pageSize: params.pageSize ?? 10,
    totalCount: 0,
    totalPages: 0,
    hasPrevious: false,
    hasNext: false
  };
}
