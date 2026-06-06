export enum DayOfWeek {
  Sunday = 'Sunday',
  Monday = 'Monday',
  Tuesday = 'Tuesday',
  Wednesday = 'Wednesday',
  Thursday = 'Thursday',
  Friday = 'Friday',
  Saturday = 'Saturday'
}

export enum AvailabilityStatus {
  Available = 'Available',
  Unavailable = 'Unavailable',
  Remote = 'Remote',
  OnSite = 'OnSite'
}

export interface BillingSettingsDto {
  defaultAnnualLeaveDays: number;
  allowHalfDayLeave: boolean;
  defaultWorkdayStart: string;
  defaultWorkdayEnd: string;
  halfDaySplitTime: string;
  defaultWorkdays: DayOfWeek[];
  defaultAvailabilityStatus: AvailabilityStatus;
  defaultWorkdayDurationMinutes: number;
  canEdit: boolean;
}

export interface UpdateBillingSettingsRequest {
  defaultAnnualLeaveDays: number;
  allowHalfDayLeave: boolean;
  defaultWorkdayStart: string;
  defaultWorkdayEnd: string;
  halfDaySplitTime: string;
  defaultWorkdays: DayOfWeek[];
  defaultAvailabilityStatus: AvailabilityStatus;
}

export interface LeaveBalanceDto {
  userId: string;
  year: number;
  entitledDays: number;
  usedDays: number;
  remainingDays: number;
}
