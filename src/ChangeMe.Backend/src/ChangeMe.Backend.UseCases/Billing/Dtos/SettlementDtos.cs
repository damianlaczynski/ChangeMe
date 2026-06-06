using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class SettlementPeriodListItemDto
{
  public Guid Id { get; set; }
  public int Year { get; set; }
  public int Month { get; set; }
  public string Label { get; set; } = string.Empty;
  public SettlementPeriodStatus Status { get; set; }
}

public class SettlementPeriodDetailsDto
{
  public Guid Id { get; set; }
  public int Year { get; set; }
  public int Month { get; set; }
  public string Label { get; set; } = string.Empty;
  public SettlementPeriodStatus Status { get; set; }
  public DateTime? ClosedAt { get; set; }
  public string? ClosedByDisplayName { get; set; }
  public DateTime? LastCalculatedAt { get; set; }
  public bool CanManage { get; set; }
  public IReadOnlyList<UserSettlementListItemDto> UserSettlements { get; set; } = [];
}

public class UserSettlementListItemDto
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string UserDisplayName { get; set; } = string.Empty;
  public string PositionName { get; set; } = string.Empty;
  public ContractType? ContractType { get; set; }
  public int ExpectedMinutes { get; set; }
  public int LoggedMinutes { get; set; }
  public decimal LeaveDays { get; set; }
  public int BalanceMinutes { get; set; }
  public DateTime LastCalculatedAt { get; set; }
  public bool CanRecalculate { get; set; }
}

public class UserSettlementDetailsDto
{
  public Guid Id { get; set; }
  public Guid SettlementPeriodId { get; set; }
  public int Year { get; set; }
  public int Month { get; set; }
  public string PeriodLabel { get; set; } = string.Empty;
  public Guid UserId { get; set; }
  public string UserDisplayName { get; set; } = string.Empty;
  public UserSettlementContractSummaryDto? Contract { get; set; }
  public UserSettlementExpectedTimeDto ExpectedTime { get; set; } = new();
  public int LoggedMinutes { get; set; }
  public IReadOnlyList<UserSettlementLeaveItemDto> LeaveItems { get; set; } = [];
  public int BalanceMinutes { get; set; }
  public string BalanceLabel { get; set; } = string.Empty;
  public bool CanRecalculate { get; set; }
}

public class UserSettlementContractSummaryDto
{
  public Guid ContractId { get; set; }
  public string PositionName { get; set; } = string.Empty;
  public ContractType ContractType { get; set; }
  public DateOnly StartDate { get; set; }
  public DateOnly? EndDate { get; set; }
  public decimal Fte { get; set; }
  public int MonthlyHoursNormMinutes { get; set; }
}

public class UserSettlementExpectedTimeDto
{
  public int MonthlyHoursNormMinutes { get; set; }
  public string? ProrationNote { get; set; }
  public int PaidLeaveMinutesDeducted { get; set; }
  public int ExpectedMinutes { get; set; }
}

public class UserSettlementLeaveItemDto
{
  public string LeaveTypeName { get; set; } = string.Empty;
  public string DatesDisplay { get; set; } = string.Empty;
  public decimal Days { get; set; }
}

public class MySettlementListItemDto
{
  public Guid Id { get; set; }
  public Guid SettlementPeriodId { get; set; }
  public int Year { get; set; }
  public int Month { get; set; }
  public string PeriodLabel { get; set; } = string.Empty;
  public int ExpectedMinutes { get; set; }
  public int LoggedMinutes { get; set; }
  public decimal LeaveDays { get; set; }
  public int BalanceMinutes { get; set; }
}

public class SettlementOperationLogListItemDto
{
  public Guid Id { get; set; }
  public DateTime Timestamp { get; set; }
  public string ActorDisplayName { get; set; } = string.Empty;
  public SettlementOperationType Operation { get; set; }
  public string PeriodLabel { get; set; } = string.Empty;
  public string? UserDisplayName { get; set; }
}

public record CreateSettlementPeriodRequest(int Year, int Month);

public class GetSettlementOperationHistoryQuery : PaginationQuery<SettlementOperationLogListItemDto>
{
  public Guid? SettlementPeriodId { get; set; }
}
