using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Enums;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class BillingSettlementReportRowDto
{
  public string Label { get; set; } = string.Empty;
  public Guid? UserId { get; set; }
  public string? UserDisplayName { get; set; }
  public string? PositionName { get; set; }
  public ContractType? ContractType { get; set; }
  public int? UserCount { get; set; }
  public int ExpectedMinutes { get; set; }
  public int LoggedMinutes { get; set; }
  public decimal LeaveDays { get; set; }
  public int BalanceMinutes { get; set; }
}

public class BillingSettlementReportResultDto
{
  public BillingSettlementReportGroupingMode GroupingMode { get; set; }
  public int PeriodYear { get; set; }
  public int PeriodMonth { get; set; }
  public string PeriodLabel { get; set; } = string.Empty;
  public int UserCount { get; set; }
  public int TotalExpectedMinutes { get; set; }
  public int TotalLoggedMinutes { get; set; }
  public int NetBalanceMinutes { get; set; }
  public IReadOnlyList<BillingSettlementReportRowDto> Rows { get; set; } = [];
}

public class BillingLeaveReportRowDto
{
  public Guid? UserId { get; set; }
  public string? UserDisplayName { get; set; }
  public string? LeaveTypeName { get; set; }
  public decimal? EntitledDays { get; set; }
  public decimal? UsedDays { get; set; }
  public decimal? RemainingDays { get; set; }
  public decimal? TotalDays { get; set; }
  public int? RequestCount { get; set; }
  public string? DatesDisplay { get; set; }
  public decimal? Days { get; set; }
}

public class BillingLeaveReportResultDto
{
  public BillingLeaveReportGroupingMode GroupingMode { get; set; }
  public int Year { get; set; }
  public IReadOnlyList<BillingLeaveReportRowDto> Rows { get; set; } = [];
}

public class GetBillingSettlementReportQuery : IQuery<BillingSettlementReportResultDto>
{
  public Guid SettlementPeriodId { get; set; }
  public List<Guid>? UserIds { get; set; }
  public List<ContractType>? ContractTypes { get; set; }
  public BillingSettlementReportGroupingMode GroupingMode { get; set; } =
    BillingSettlementReportGroupingMode.ByPerson;
}

public class GetBillingLeaveReportQuery : IQuery<BillingLeaveReportResultDto>
{
  public int Year { get; set; }
  public List<Guid>? UserIds { get; set; }
  public List<Guid>? LeaveTypeIds { get; set; }
  public List<LeaveRequestStatus>? Statuses { get; set; }
  public BillingLeaveReportGroupingMode GroupingMode { get; set; } =
    BillingLeaveReportGroupingMode.ByPerson;
}
