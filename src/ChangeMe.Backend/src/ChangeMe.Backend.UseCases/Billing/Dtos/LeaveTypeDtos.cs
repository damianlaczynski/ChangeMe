using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class LeaveTypeListItemDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Code { get; set; } = string.Empty;
  public bool CountsAsPaid { get; set; }
  public bool UsesAllowance { get; set; }
  public bool RequiresApproval { get; set; }
  public bool IsActive { get; set; }
  public bool IsSeeded { get; set; }
  public bool CanManage { get; set; }
  public bool CanDelete { get; set; }
}

public class LeaveTypeDetailsDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Code { get; set; } = string.Empty;
  public bool CountsAsPaid { get; set; }
  public bool UsesAllowance { get; set; }
  public bool RequiresApproval { get; set; }
  public bool IsActive { get; set; }
  public bool IsSeeded { get; set; }
  public bool CanManage { get; set; }
  public bool CanDelete { get; set; }
}

public record CreateLeaveTypeRequest(
  string Name,
  string Code,
  bool CountsAsPaid,
  bool UsesAllowance,
  bool RequiresApproval,
  bool IsActive = true);

public record UpdateLeaveTypeRequest(
  string Name,
  string Code,
  bool CountsAsPaid,
  bool UsesAllowance,
  bool RequiresApproval,
  bool IsActive);

public class BillingSettingsDto
{
  public decimal DefaultAnnualLeaveDays { get; set; }
  public bool AllowHalfDayLeave { get; set; }
  public string DefaultWorkdayStart { get; set; } = string.Empty;
  public string DefaultWorkdayEnd { get; set; } = string.Empty;
  public string HalfDaySplitTime { get; set; } = string.Empty;
  public IReadOnlyList<DayOfWeek> DefaultWorkdays { get; set; } = [];
  public AvailabilityStatus DefaultAvailabilityStatus { get; set; }
  public int DefaultWorkdayDurationMinutes { get; set; }
  public bool CanEdit { get; set; }
}

public record UpdateBillingSettingsRequest(
  decimal DefaultAnnualLeaveDays,
  bool AllowHalfDayLeave,
  string DefaultWorkdayStart,
  string DefaultWorkdayEnd,
  string HalfDaySplitTime,
  IReadOnlyList<DayOfWeek> DefaultWorkdays,
  AvailabilityStatus DefaultAvailabilityStatus);

public class LeaveBalanceDto
{
  public Guid UserId { get; set; }
  public int Year { get; set; }
  public decimal EntitledDays { get; set; }
  public decimal UsedDays { get; set; }
  public decimal RemainingDays { get; set; }
}
