import {
  AvailabilityStatus,
  DayOfWeek
} from '@features/billing/models/billing-settings.model';
import {
  ContractStatus,
  ContractType
} from '@features/billing/models/employment.model';
import {
  LeaveDayPortion,
  LeaveRequestStatus
} from '@features/billing/models/leave-request.model';
import { SettlementPeriodStatus } from '@features/billing/models/settlement.model';

export const PositionConstraints = {
  NAME_MIN_LENGTH: 2,
  NAME_MAX_LENGTH: 100,
  DEPARTMENT_MAX_LENGTH: 100,
  DESCRIPTION_MAX_LENGTH: 500
} as const;

export const EmploymentConstraints = {
  EMPLOYEE_ID_MAX_LENGTH: 50,
  NATIONAL_ID_MAX_LENGTH: 20,
  TAX_ID_MAX_LENGTH: 20,
  BANK_ACCOUNT_MAX_LENGTH: 34,
  EMPLOYMENT_NOTES_MAX_LENGTH: 500,
  CONTRACT_NOTES_MAX_LENGTH: 500,
  MIN_FTE: 0.01,
  MAX_FTE: 1,
  MIN_MONTHLY_HOURS_NORM_MINUTES: 60,
  MAX_MONTHLY_HOURS_NORM_MINUTES: 10080,
  MIN_COMPENSATION: 0.01
} as const;

export const BillingMessages = {
  positionCreated: 'Position created.',
  positionSaved: 'Position saved.',
  positionDeleted: 'Position deleted.',
  duplicateName: 'A position with this name already exists.',
  noPositions: 'No positions defined.',
  deletePositionConfirm: (name: string) => `Delete position "${name}"?`,
  departmentEmpty: '—',
  descriptionEmpty: '—',
  employmentProfileSaved: 'Employment profile saved.',
  contractCreated: 'Contract created.',
  contractSaved: 'Contract saved.',
  duplicateEmployeeId: 'An employee with this employee ID already exists.',
  contractOverlap: 'Contract dates overlap an existing contract.',
  compensationRequired: 'Enter an hourly rate or a monthly salary.',
  noContracts: 'No contracts defined.',
  emptyField: '—',
  billingSettingsSaved: 'Billing settings saved.',
  leaveTypeCreated: 'Leave type created.',
  leaveTypeSaved: 'Leave type saved.',
  leaveTypeDeleted: 'Leave type deleted.',
  duplicateLeaveTypeName: 'A leave type with this name already exists.',
  duplicateLeaveTypeCode: 'A leave type with this code already exists.',
  deleteLeaveTypeConfirm: (name: string) => `Delete leave type "${name}"?`,
  noLeaveTypes: 'No leave types defined.',
  annualLeaveDaysInvalid:
    'Enter a number from 0 to 365 with at most one decimal place.',
  invalidTime: 'Enter a valid time.',
  workdayEndBeforeStart: 'Default workday end must be after Default workday start.',
  halfDaySplitOutOfRange: 'Half-day split time must be between workday start and end.',
  defaultWorkdaysRequired: 'Select at least one default workday.',
  billingReportsPlaceholder: 'Select filters and run a report.',
  noReportData: 'No data matches the selected filters.',
  reportExported: 'Report exported.',
  noSettlementOperations: 'No settlement operations recorded.',
  billingTabNotImplemented: 'This tab will be available in a later release.',
  leaveRequestSaved: 'Leave request saved.',
  leaveRequestSubmitted: 'Leave request submitted.',
  leaveRequestApproved: 'Leave request approved.',
  leaveRequestRejected: 'Leave request rejected.',
  leaveRequestCancelled: 'Leave request cancelled.',
  leaveRequestDeleted: 'Leave request deleted.',
  leaveOverlap: 'Leave dates overlap an existing request.',
  noLeaveRequests: 'No leave requests match the filters.',
  noMyLeaveRequests: 'You have no leave requests.',
  noActiveContract:
    'No active employment contract. Leave entitlement is not calculated.',
  cancelLeaveRequestConfirm: 'Cancel this leave request?',
  deleteDraftLeaveRequestConfirm: 'Delete this draft leave request?',
  rejectLeaveRequest: 'Reject leave request',
  settlementPeriodCreated: 'Settlement period created.',
  settlementsRecalculated: 'Settlements recalculated.',
  settlementRecalculated: 'Settlement recalculated.',
  settlementPeriodClosed: 'Settlement period closed.',
  duplicateSettlementPeriod: 'A settlement period for this month already exists.',
  noSettlementsForPeriod: 'No settlements for this period.',
  noPublishedSettlements: 'No published settlements yet.',
  noSettlementContract: 'No active contract',
  noSettlementLeave: 'No approved leave in this period.',
  closeSettlementPeriodConfirm: (label: string) =>
    `Close ${label}? Settlements cannot be recalculated after closing.`,
  availabilitySaved: 'Availability saved.',
  availabilityDeleted: 'Availability deleted.',
  weeklyPatternSaved: 'Weekly pattern saved.',
  weeklyPatternReset: 'Weekly pattern reset to organization defaults.',
  availabilityOverlap: 'Availability overlaps an existing entry.',
  deleteAvailabilityConfirm: 'Delete this availability entry?',
  resetWeeklyPatternConfirm:
    'Replace your weekly pattern with organization defaults scaled to your current contract FTE?',
  noUsersMatchFilters: 'No users match the selected filters.',
  teamCalendarTruncated: 'Showing the first 50 users. Narrow the filter to see others.',
  recurringPatternHint: 'From weekly pattern.'
} as const;

export const LeaveRequestConstraints = {
  REASON_MAX_LENGTH: 500,
  REJECT_REASON_MAX_LENGTH: 500,
  PAGE_SIZE: 20
} as const;

export const SettlementConstraints = {
  YEAR_OFFSET: 2,
  MIN_MONTH: 1,
  MAX_MONTH: 12
} as const;

export const LeaveTypeConstraints = {
  NAME_MIN_LENGTH: 2,
  NAME_MAX_LENGTH: 100,
  CODE_MIN_LENGTH: 2,
  CODE_MAX_LENGTH: 20
} as const;

export const BillingSettingsConstraints = {
  MIN_ANNUAL_LEAVE_DAYS: 0,
  MAX_ANNUAL_LEAVE_DAYS: 365
} as const;

export function getPositionActiveLabel(isActive: boolean): string {
  return isActive ? 'Active' : 'Inactive';
}

export function getPositionActiveSeverity(isActive: boolean): 'success' | 'secondary' {
  return isActive ? 'success' : 'secondary';
}

export function getContractTypeLabel(type: ContractType): string {
  switch (type) {
    case ContractType.Employment:
      return 'Employment';
    case ContractType.Mandate:
      return 'Mandate';
    case ContractType.WorkContract:
      return 'Work contract';
    case ContractType.B2B:
      return 'B2B';
    default:
      return type;
  }
}

export function getContractStatusSeverity(
  status: ContractStatus
): 'success' | 'info' | 'secondary' {
  switch (status) {
    case ContractStatus.Active:
      return 'success';
    case ContractStatus.Future:
      return 'info';
    case ContractStatus.Ended:
      return 'secondary';
    default:
      return 'secondary';
  }
}

export function formatMonthlyHoursNorm(minutes: number): string {
  const hours = Math.floor(Math.abs(minutes) / 60);
  const remainingMinutes = Math.abs(minutes) % 60;
  const formatted = `${hours}h ${remainingMinutes}m`;
  return minutes < 0 ? `-${formatted}` : formatted;
}

export function formatCompensationDisplay(
  hourlyRate?: number | null,
  monthlySalary?: number | null
): string {
  if (hourlyRate != null) {
    return `${hourlyRate.toFixed(2)}/h`;
  }

  if (monthlySalary != null) {
    return `${monthlySalary.toFixed(2)}/mo`;
  }

  return BillingMessages.emptyField;
}

export function formatEmploymentField(value?: string | null): string {
  return value?.trim() ? value.trim() : BillingMessages.emptyField;
}

export const contractTypeOptions = [
  { label: 'Employment', value: ContractType.Employment },
  { label: 'Mandate', value: ContractType.Mandate },
  { label: 'Work contract', value: ContractType.WorkContract },
  { label: 'B2B', value: ContractType.B2B }
];

export function splitDurationMinutes(totalMinutes: number): {
  hours: number;
  minutes: number;
} {
  return {
    hours: Math.floor(totalMinutes / 60),
    minutes: totalMinutes % 60
  };
}

export function combineDurationMinutes(hours: number, minutes: number): number {
  return hours * 60 + minutes;
}

export const weekdayOptions = [
  { label: 'Monday', value: DayOfWeek.Monday },
  { label: 'Tuesday', value: DayOfWeek.Tuesday },
  { label: 'Wednesday', value: DayOfWeek.Wednesday },
  { label: 'Thursday', value: DayOfWeek.Thursday },
  { label: 'Friday', value: DayOfWeek.Friday },
  { label: 'Saturday', value: DayOfWeek.Saturday },
  { label: 'Sunday', value: DayOfWeek.Sunday }
];

export const availabilityStatusOptions = [
  { label: 'Available', value: AvailabilityStatus.Available },
  { label: 'Unavailable', value: AvailabilityStatus.Unavailable },
  { label: 'Remote', value: AvailabilityStatus.Remote },
  { label: 'On-site', value: AvailabilityStatus.OnSite }
];

export const recurringAvailabilityStatusOptions = availabilityStatusOptions.filter(
  (option) => option.value !== AvailabilityStatus.Unavailable
);

export const availabilityStatusFilterOptions = [
  ...availabilityStatusOptions,
  { label: 'Leave', value: 'Leave' as const }
];

export function formatBooleanLabel(value: boolean): string {
  return value ? 'Yes' : 'No';
}

export function formatWorkdayDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;
  return `${hours}h ${remainingMinutes}m`;
}

export function isValidTimeString(value: string): boolean {
  return /^([01]\d|2[0-3]):[0-5]\d$/.test(value.trim());
}

export function compareTimeStrings(start: string, end: string): number {
  const [startH, startM] = start.split(':').map(Number);
  const [endH, endM] = end.split(':').map(Number);
  return endH * 60 + endM - (startH * 60 + startM);
}

export const leaveRequestStatusOptions = [
  { label: 'Draft', value: LeaveRequestStatus.Draft },
  { label: 'Submitted', value: LeaveRequestStatus.Submitted },
  { label: 'Approved', value: LeaveRequestStatus.Approved },
  { label: 'Rejected', value: LeaveRequestStatus.Rejected },
  { label: 'Cancelled', value: LeaveRequestStatus.Cancelled }
];

export const leaveDayPortionOptions = [
  { label: 'Full day', value: LeaveDayPortion.FullDay },
  { label: 'First half', value: LeaveDayPortion.FirstHalf },
  { label: 'Second half', value: LeaveDayPortion.SecondHalf }
];

export function getLeaveRequestStatusSeverity(
  status: LeaveRequestStatus
): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
  switch (status) {
    case LeaveRequestStatus.Approved:
      return 'success';
    case LeaveRequestStatus.Submitted:
      return 'info';
    case LeaveRequestStatus.Rejected:
      return 'danger';
    case LeaveRequestStatus.Cancelled:
      return 'secondary';
    case LeaveRequestStatus.Draft:
    default:
      return 'warn';
  }
}

export function getLeaveDayPortionLabel(portion?: LeaveDayPortion | null): string {
  if (!portion) {
    return BillingMessages.emptyField;
  }

  return (
    leaveDayPortionOptions.find((option) => option.value === portion)?.label ?? portion
  );
}

export function getCurrentMonthDateRange(): { from: Date; to: Date } {
  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth(), 1);
  const to = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  return { from, to };
}

export function getNextMonthDateRange(): { from: Date; to: Date } {
  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth() + 1, 1);
  const to = new Date(now.getFullYear(), now.getMonth() + 2, 0);
  return { from, to };
}

export function getCurrentQuarterDateRange(): { from: Date; to: Date } {
  const now = new Date();
  const quarterStartMonth = Math.floor(now.getMonth() / 3) * 3;
  const from = new Date(now.getFullYear(), quarterStartMonth, 1);
  const to = new Date(now.getFullYear(), quarterStartMonth + 3, 0);
  return { from, to };
}

export type LeaveDatePresetId = 'thisMonth' | 'nextMonth' | 'thisQuarter';

export const leaveDateFilterPresets: { id: LeaveDatePresetId; label: string }[] = [
  { id: 'thisMonth', label: 'This month' },
  { id: 'nextMonth', label: 'Next month' },
  { id: 'thisQuarter', label: 'This quarter' }
];

export function getLeaveDateRangeForPreset(presetId: LeaveDatePresetId): {
  from: Date;
  to: Date;
} {
  switch (presetId) {
    case 'nextMonth':
      return getNextMonthDateRange();
    case 'thisQuarter':
      return getCurrentQuarterDateRange();
    case 'thisMonth':
    default:
      return getCurrentMonthDateRange();
  }
}

export function getSettlementPeriodStatusSeverity(
  status: SettlementPeriodStatus
): 'success' | 'secondary' {
  return status === SettlementPeriodStatus.Open ? 'success' : 'secondary';
}

export function getSettlementBalanceSeverity(
  balanceMinutes: number
): 'success' | 'warn' | 'secondary' {
  if (balanceMinutes < 0) {
    return 'warn';
  }

  if (balanceMinutes > 0) {
    return 'success';
  }

  return 'secondary';
}

export function getSettlementBalanceClass(balanceMinutes: number): string {
  if (balanceMinutes < 0) {
    return 'text-orange-500';
  }

  if (balanceMinutes > 0) {
    return 'text-green-600 dark:text-green-400';
  }

  return '';
}

export function getSettlementYearRange(): { min: number; max: number } {
  const currentYear = new Date().getFullYear();
  return {
    min: currentYear - SettlementConstraints.YEAR_OFFSET,
    max: currentYear + SettlementConstraints.YEAR_OFFSET
  };
}

export const settlementMonthOptions = Array.from({ length: 12 }, (_, index) => {
  const month = index + 1;
  const label = new Date(2000, index, 1).toLocaleString(undefined, { month: 'long' });
  return { label, value: month };
});
