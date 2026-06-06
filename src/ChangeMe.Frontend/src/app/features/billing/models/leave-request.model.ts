export enum LeaveRequestStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Cancelled = 'Cancelled'
}

export enum LeaveDayPortion {
  FullDay = 'FullDay',
  FirstHalf = 'FirstHalf',
  SecondHalf = 'SecondHalf'
}

export interface LeaveRequestListItemDto {
  id: string;
  userId: string;
  userDisplayName: string;
  leaveTypeName: string;
  datesDisplay: string;
  days: number;
  status: LeaveRequestStatus;
  submittedAt?: string | null;
  startDate: string;
}

export interface LeaveRequestDetailsDto {
  id: string;
  userId: string;
  userDisplayName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  dayPortion?: LeaveDayPortion | null;
  days: number;
  status: LeaveRequestStatus;
  reason?: string | null;
  submittedAt?: string | null;
  decidedAt?: string | null;
  decidedByUserId?: string | null;
  decidedByDisplayName?: string | null;
  rejectReason?: string | null;
  canEdit: boolean;
  canSubmit: boolean;
  canApprove: boolean;
  canReject: boolean;
  canCancel: boolean;
  canDelete: boolean;
}

export interface SaveLeaveRequestRequest {
  userId?: string | null;
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  dayPortion?: LeaveDayPortion | null;
  reason?: string | null;
  submit?: boolean;
}

export interface LeaveRequestSearchParameters {
  pageNumber: number;
  pageSize: number;
  sortField?: string;
  ascending?: boolean;
  statuses?: LeaveRequestStatus[];
  leaveTypeIds?: string[];
  userIds?: string[];
  dateFrom?: string;
  dateTo?: string;
}

export interface MyLeaveRequestSearchParameters {
  pageNumber: number;
  pageSize: number;
  sortField?: string;
  ascending?: boolean;
  showAllYears?: boolean;
}

export interface RejectLeaveRequestRequest {
  rejectReason: string;
}
