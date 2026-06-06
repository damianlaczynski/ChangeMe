export enum ContractType {
  Employment = 'Employment',
  Mandate = 'Mandate',
  WorkContract = 'WorkContract',
  B2B = 'B2B'
}

export enum ContractStatus {
  Active = 'Active',
  Future = 'Future',
  Ended = 'Ended'
}

export interface EmploymentProfileDto {
  employeeId?: string | null;
  nationalId?: string | null;
  taxId?: string | null;
  bankAccount?: string | null;
  notes?: string | null;
  canManage: boolean;
}

export interface EmploymentContractListItemDto {
  id: string;
  positionName: string;
  contractType: ContractType;
  startDate: string;
  endDate?: string | null;
  fte: number;
  monthlyHoursNormMinutes: number;
  rateOrSalaryDisplay: string;
  status: ContractStatus;
}

export interface UserEmploymentDto {
  profile: EmploymentProfileDto;
  contracts: EmploymentContractListItemDto[];
  isExpandedByDefault: boolean;
}

export interface EmploymentContractDetailsDto {
  id: string;
  userId: string;
  userDisplayName: string;
  positionId: string;
  positionName: string;
  contractType: ContractType;
  startDate: string;
  endDate?: string | null;
  fte: number;
  monthlyHoursNormMinutes: number;
  hourlyRate?: number | null;
  monthlySalary?: number | null;
  notes?: string | null;
  status: ContractStatus;
  canManage: boolean;
}

export interface UpsertEmploymentProfileRequest {
  employeeId?: string | null;
  nationalId?: string | null;
  taxId?: string | null;
  bankAccount?: string | null;
  notes?: string | null;
}

export interface SaveEmploymentContractRequest {
  positionId: string;
  contractType: ContractType;
  startDate: string;
  endDate?: string | null;
  fte: number;
  monthlyHoursNormMinutes: number;
  hourlyRate?: number | null;
  monthlySalary?: number | null;
  notes?: string | null;
}

export interface UpdateEmploymentContractRequest extends SaveEmploymentContractRequest {
  userId: string;
}

export interface MyEmploymentSummaryDto {
  positionName: string;
  contractType: ContractType;
  startDate: string;
  endDate?: string | null;
  fte: number;
  monthlyHoursNormMinutes: number;
}

export interface PositionOptionDto {
  id: string;
  name: string;
}
