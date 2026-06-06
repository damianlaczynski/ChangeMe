import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface PositionListItemDto {
  id: string;
  name: string;
  department?: string | null;
  isActive: boolean;
  contractCount: number;
  canManage: boolean;
}

export interface PositionDetailsDto {
  id: string;
  name: string;
  department?: string | null;
  description?: string | null;
  isActive: boolean;
  contractCount: number;
  canManage: boolean;
  canDelete: boolean;
}

export interface PositionSearchParameters extends PaginationParameters {
  searchText?: string;
}

export interface CreatePositionRequest {
  name: string;
  department?: string | null;
  description?: string | null;
  isActive?: boolean;
}

export interface UpdatePositionRequest {
  name: string;
  department?: string | null;
  description?: string | null;
  isActive: boolean;
}
