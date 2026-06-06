import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  AvailabilityCalendarResultDto,
  AvailabilityDayResultDto,
  AvailabilityEntryDto,
  CreateAvailabilityEntryRequest,
  TeamAvailabilityCalendarParameters,
  UpdateAvailabilityEntryRequest,
  WeeklyRecurringPatternDayDto,
  WeeklyRecurringPatternDto
} from '../models/availability.model';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityService {
  private readonly apiService = inject(ApiService);

  getMyCalendar(from: string, to: string): Observable<AvailabilityCalendarResultDto> {
    return this.apiService.get<AvailabilityCalendarResultDto>(
      'billing/my/availability/calendar',
      { from, to }
    );
  }

  getTeamCalendar(
    params: TeamAvailabilityCalendarParameters
  ): Observable<AvailabilityCalendarResultDto> {
    return this.apiService.get<AvailabilityCalendarResultDto>(
      'billing/availability/calendar',
      params
    );
  }

  getMyDay(date: string): Observable<AvailabilityDayResultDto> {
    return this.apiService.get<AvailabilityDayResultDto>(
      `billing/my/availability/days/${date}`
    );
  }

  getUserDay(userId: string, date: string): Observable<AvailabilityDayResultDto> {
    return this.apiService.get<AvailabilityDayResultDto>(
      `billing/users/${userId}/availability/days/${date}`
    );
  }

  getMyPattern(): Observable<WeeklyRecurringPatternDto> {
    return this.apiService.get<WeeklyRecurringPatternDto>(
      'billing/my/availability/pattern'
    );
  }

  getUserPattern(userId: string): Observable<WeeklyRecurringPatternDto> {
    return this.apiService.get<WeeklyRecurringPatternDto>(
      `billing/users/${userId}/availability/pattern`
    );
  }

  saveMyPattern(
    days: WeeklyRecurringPatternDayDto[]
  ): Observable<WeeklyRecurringPatternDto> {
    return this.apiService.put<WeeklyRecurringPatternDto>(
      'billing/my/availability/pattern',
      {
        days
      }
    );
  }

  saveUserPattern(
    userId: string,
    days: WeeklyRecurringPatternDayDto[]
  ): Observable<WeeklyRecurringPatternDto> {
    return this.apiService.put<WeeklyRecurringPatternDto>(
      `billing/users/${userId}/availability/pattern`,
      { days }
    );
  }

  resetMyPattern(): Observable<WeeklyRecurringPatternDto> {
    return this.apiService.post<WeeklyRecurringPatternDto>(
      'billing/my/availability/pattern/reset',
      {}
    );
  }

  resetUserPattern(userId: string): Observable<WeeklyRecurringPatternDto> {
    return this.apiService.post<WeeklyRecurringPatternDto>(
      `billing/users/${userId}/availability/pattern/reset`,
      {}
    );
  }

  createMyEntry(
    request: CreateAvailabilityEntryRequest
  ): Observable<AvailabilityEntryDto> {
    return this.apiService.post<AvailabilityEntryDto>(
      'billing/my/availability/entries',
      request
    );
  }

  createUserEntry(
    request: CreateAvailabilityEntryRequest
  ): Observable<AvailabilityEntryDto> {
    return this.apiService.post<AvailabilityEntryDto>(
      'billing/availability/entries',
      request
    );
  }

  updateEntry(
    id: string,
    request: UpdateAvailabilityEntryRequest
  ): Observable<AvailabilityEntryDto> {
    return this.apiService.put<AvailabilityEntryDto>(
      `billing/availability/entries/${id}`,
      request
    );
  }

  deleteEntry(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`billing/availability/entries/${id}`);
  }
}
