namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public static class BillingConstraints
{
  public const int PositionNameMinLength = 2;
  public const int PositionNameMaxLength = 100;
  public const int PositionDepartmentMaxLength = 100;
  public const int PositionDescriptionMaxLength = 500;

  public const int EmployeeIdMaxLength = 50;
  public const int NationalIdMaxLength = 20;
  public const int TaxIdMaxLength = 20;
  public const int BankAccountMaxLength = 34;
  public const int EmploymentNotesMaxLength = 500;
  public const int ContractNotesMaxLength = 500;

  public const decimal MinFte = 0.01m;
  public const decimal MaxFte = 1.00m;
  public const int MinMonthlyHoursNormMinutes = 60;
  public const int MaxMonthlyHoursNormMinutes = 10080;
  public const decimal MinCompensationAmount = 0.01m;

  public const int LeaveTypeNameMinLength = 2;
  public const int LeaveTypeNameMaxLength = 100;
  public const int LeaveTypeCodeMinLength = 2;
  public const int LeaveTypeCodeMaxLength = 20;

  public const int LeaveReasonMaxLength = 500;
  public const int LeaveRejectReasonMaxLength = 500;

  public const decimal MinAnnualLeaveDays = 0m;
  public const decimal MaxAnnualLeaveDays = 365m;

  public const int AvailabilityNotesMaxLength = 500;

  public const string PositionNameDuplicateMessage = "A position with this name already exists.";
  public const string PositionReferencedMessage = "Position is referenced by employment contracts.";
  public const string ContractOverlapMessage = "Contract dates overlap an existing contract.";
  public const string EmployeeIdDuplicateMessage = "An employee with this employee ID already exists.";
  public const string InactivePositionMessage = "Selected position is not active.";
  public const string EmploymentProfileSavedMessage = "Employment profile saved.";
  public const string ContractCreatedMessage = "Contract created.";
  public const string ContractSavedMessage = "Contract saved.";
  public const string ContractCompensationRequiredMessage = "Enter an hourly rate or a monthly salary.";
  public const string LeaveTypeNameDuplicateMessage = "A leave type with this name already exists.";
  public const string LeaveTypeCodeDuplicateMessage = "A leave type with this code already exists.";
  public const string SeededLeaveTypeDeleteMessage = "Seeded leave types cannot be deleted.";
  public const string LeaveTypeReferencedMessage = "Leave type is referenced by leave requests.";
  public const string SettlementPeriodDuplicateMessage = "A settlement period for this month already exists.";
  public const string SettlementPeriodClosedMessage = "Settlement period is closed.";
  public const string CannotApproveOwnLeaveMessage = "You cannot approve your own leave request.";
  public const string LeaveOverlapMessage = "Leave dates overlap an existing request.";
  public const int LeaveBackdatingMonths = 12;
  public const string AvailabilityOverlapMessage = "Availability overlaps an existing entry.";
  public const string DefaultWorkdayEndBeforeStartMessage = "Default workday end must be after Default workday start.";
  public const string HalfDaySplitOutOfRangeMessage = "Half-day split time must be between workday start and end.";
  public const string DefaultWorkdaysRequiredMessage = "Select at least one default workday.";
  public const string AnnualLeaveDaysRangeMessage = "Enter a number from 0 to 365 with at most one decimal place.";
}
