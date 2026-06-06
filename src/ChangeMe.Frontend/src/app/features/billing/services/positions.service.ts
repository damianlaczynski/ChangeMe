import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  CreatePositionRequest,
  PositionDetailsDto,
  PositionListItemDto,
  PositionSearchParameters,
  UpdatePositionRequest
} from '../models/position.model';

@Injectable({
  providedIn: 'root'
})
export class PositionsService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'billing/positions';

  getPositions(
    params: PositionSearchParameters
  ): Observable<PaginationResult<PositionListItemDto>> {
    return this.apiService.getPaginated<PositionListItemDto, PositionSearchParameters>(
      this.baseEndpoint,
      params
    );
  }

  getPositionById(id: string): Observable<PositionDetailsDto> {
    return this.apiService.get<PositionDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  createPosition(request: CreatePositionRequest): Observable<PositionDetailsDto> {
    return this.apiService.post<PositionDetailsDto>(this.baseEndpoint, request);
  }

  updatePosition(
    id: string,
    request: UpdatePositionRequest
  ): Observable<PositionDetailsDto> {
    return this.apiService.put<PositionDetailsDto>(
      `${this.baseEndpoint}/${id}`,
      request
    );
  }

  deletePosition(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }
}
