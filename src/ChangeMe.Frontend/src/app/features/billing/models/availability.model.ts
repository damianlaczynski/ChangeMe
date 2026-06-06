import { DayOfWeek } from '@features/billing/models/billing-settings.model';

export enum AvailabilityEntrySource {
  Manual = 'Manual',
  Recurring = 'Recurring',
  Leave = 'Leave'
}

export enum AvailabilityStatus {
  Available = 'Available',
  Unavailable = 'Unavailable',
  Remote = 'Remote',
  OnSite = 'OnSite'
}

export type AvailabilityCalendarView = 'month' | 'week';
export type AvailabilityCalendarDensity = 'compact' | 'standard';

export interface AvailabilityEntryDto {
  id: string;
  userId: string;
  startDate: string;
  endDate: string;
  allDay: boolean;
  startTime?: string | null;
  endTime?: string | null;
  status: AvailabilityStatus;
  notes: string;
  source: AvailabilityEntrySource;
  leaveRequestId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface WeeklyRecurringPatternDayDto {
  dayOfWeek: DayOfWeek;
  enabled: boolean;
  startTime?: string | null;
  endTime?: string | null;
  status?: AvailabilityStatus | null;
}

export interface WeeklyRecurringPatternDto {
  userId: string;
  days: WeeklyRecurringPatternDayDto[];
  canEdit: boolean;
}

export interface AvailabilityCalendarUserDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
}

export interface AvailabilityCalendarResultDto {
  from: string;
  to: string;
  users: AvailabilityCalendarUserDto[];
  entries: AvailabilityEntryDto[];
  isTruncated: boolean;
}

export interface AvailabilityDayResultDto {
  date: string;
  userId: string;
  entries: AvailabilityEntryDto[];
  canManage: boolean;
}

export interface CreateAvailabilityEntryRequest {
  userId?: string;
  startDate: string;
  endDate: string;
  allDay: boolean;
  startTime?: string | null;
  endTime?: string | null;
  status: AvailabilityStatus;
  notes?: string | null;
}

export interface UpdateAvailabilityEntryRequest {
  startDate: string;
  endDate: string;
  allDay: boolean;
  startTime?: string | null;
  endTime?: string | null;
  status: AvailabilityStatus;
  notes?: string | null;
}

export interface TeamAvailabilityCalendarParameters {
  from: string;
  to: string;
  userIds?: string[];
  projectIds?: string[];
  statuses?: string[];
}

export type AvailabilityStatusFilter = AvailabilityStatus | 'Leave';
