export interface LeaveTypeListItemDto {
  id: string;
  name: string;
  code: string;
  countsAsPaid: boolean;
  usesAllowance: boolean;
  requiresApproval: boolean;
  isActive: boolean;
  isSeeded: boolean;
  canManage: boolean;
  canDelete: boolean;
}

export interface LeaveTypeDetailsDto extends LeaveTypeListItemDto {}

export interface SaveLeaveTypeRequest {
  name: string;
  code: string;
  countsAsPaid: boolean;
  usesAllowance: boolean;
  requiresApproval: boolean;
  isActive: boolean;
}
