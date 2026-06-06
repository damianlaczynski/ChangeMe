using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class EmploymentProfileDto
{
  public string? EmployeeId { get; set; }
  public string? NationalId { get; set; }
  public string? TaxId { get; set; }
  public string? BankAccount { get; set; }
  public string? Notes { get; set; }
  public bool CanManage { get; set; }
}

public class EmploymentContractListItemDto
{
  public Guid Id { get; set; }
  public string PositionName { get; set; } = string.Empty;
  public ContractType ContractType { get; set; }
  public DateOnly StartDate { get; set; }
  public DateOnly? EndDate { get; set; }
  public decimal Fte { get; set; }
  public int MonthlyHoursNormMinutes { get; set; }
  public string RateOrSalaryDisplay { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
}

public class UserEmploymentDto
{
  public EmploymentProfileDto Profile { get; set; } = new();
  public IReadOnlyList<EmploymentContractListItemDto> Contracts { get; set; } = [];
  public bool IsExpandedByDefault { get; set; }
}

public class EmploymentContractDetailsDto
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string UserDisplayName { get; set; } = string.Empty;
  public Guid PositionId { get; set; }
  public string PositionName { get; set; } = string.Empty;
  public ContractType ContractType { get; set; }
  public DateOnly StartDate { get; set; }
  public DateOnly? EndDate { get; set; }
  public decimal Fte { get; set; }
  public int MonthlyHoursNormMinutes { get; set; }
  public decimal? HourlyRate { get; set; }
  public decimal? MonthlySalary { get; set; }
  public string? Notes { get; set; }
  public string Status { get; set; } = string.Empty;
  public bool CanManage { get; set; }
}

public record UpsertEmploymentProfileRequest(
  string? EmployeeId,
  string? NationalId,
  string? TaxId,
  string? BankAccount,
  string? Notes);

public record CreateEmploymentContractRequest(
  Guid PositionId,
  ContractType ContractType,
  DateOnly StartDate,
  DateOnly? EndDate,
  decimal Fte,
  int MonthlyHoursNormMinutes,
  decimal? HourlyRate,
  decimal? MonthlySalary,
  string? Notes);

public record UpdateEmploymentContractRequest(
  Guid UserId,
  Guid PositionId,
  ContractType ContractType,
  DateOnly StartDate,
  DateOnly? EndDate,
  decimal Fte,
  int MonthlyHoursNormMinutes,
  decimal? HourlyRate,
  decimal? MonthlySalary,
  string? Notes);

public class MyEmploymentSummaryDto
{
  public string PositionName { get; set; } = string.Empty;
  public ContractType ContractType { get; set; }
  public DateOnly StartDate { get; set; }
  public DateOnly? EndDate { get; set; }
  public decimal Fte { get; set; }
  public int MonthlyHoursNormMinutes { get; set; }
}
