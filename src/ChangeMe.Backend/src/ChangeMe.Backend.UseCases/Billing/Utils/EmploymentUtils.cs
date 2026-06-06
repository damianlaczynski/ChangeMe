using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class EmploymentUtils
{
  public const string ContractStatusActive = "Active";
  public const string ContractStatusFuture = "Future";
  public const string ContractStatusEnded = "Ended";

  public static async Task<Result> EnsureUserExistsAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var exists = await context.Users.AsNoTracking().AnyAsync(u => u.Id == userId, cancellationToken);
    return exists ? Result.Success() : Result.NotFound();
  }

  public static async Task<Result> EnsureUniqueEmployeeIdAsync(
    ApplicationDbContext context,
    string? employeeId,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var normalized = EmploymentProfile.NormalizeEmployeeId(employeeId);
    if (string.IsNullOrEmpty(normalized))
      return Result.Success();

    var taken = await context.EmploymentProfiles
      .AsNoTracking()
      .AnyAsync(
        p => p.NormalizedEmployeeId == normalized && p.UserId != userId,
        cancellationToken);
    return taken ? Result.Conflict(BillingConstraints.EmployeeIdDuplicateMessage) : Result.Success();
  }

  public static async Task<Result> EnsureActivePositionAsync(
    ApplicationDbContext context,
    Guid positionId,
    CancellationToken cancellationToken)
  {
    var position = await context.Positions
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);
    if (position is null)
      return Result.NotFound();

    return position.IsActive
      ? Result.Success()
      : Result.Invalid(new ValidationError(nameof(Position), BillingConstraints.InactivePositionMessage));
  }

  public static async Task<Result> EnsureNoContractOverlapAsync(
    ApplicationDbContext context,
    Guid userId,
    DateOnly startDate,
    DateOnly? endDate,
    Guid? excludeContractId,
    CancellationToken cancellationToken)
  {
    var existingContracts = await context.EmploymentContracts
      .AsNoTracking()
      .Where(c => c.UserId == userId && (excludeContractId == null || c.Id != excludeContractId))
      .Select(c => new { c.StartDate, c.EndDate })
      .ToListAsync(cancellationToken);

    foreach (var contract in existingContracts)
    {
      if (EmploymentContract.Overlaps(startDate, endDate, contract.StartDate, contract.EndDate))
        return Result.Conflict(BillingConstraints.ContractOverlapMessage);
    }

    return Result.Success();
  }

  public static string GetContractStatus(DateOnly startDate, DateOnly? endDate, DateOnly today)
  {
    if (startDate > today)
      return ContractStatusFuture;

    if (endDate.HasValue && endDate.Value < today)
      return ContractStatusEnded;

    return ContractStatusActive;
  }

  public static string FormatRateOrSalary(decimal? hourlyRate, decimal? monthlySalary)
  {
    if (hourlyRate.HasValue)
      return $"{hourlyRate.Value:0.00}/h";

    if (monthlySalary.HasValue)
      return $"{monthlySalary.Value:0.00}/mo";

    return string.Empty;
  }

  public static string FormatMonthlyHoursNorm(int minutes)
  {
    var hours = minutes / 60;
    var remainingMinutes = minutes % 60;
    return $"{hours}h {remainingMinutes}m";
  }

  public static string? MaskBankAccount(string? bankAccount)
  {
    if (string.IsNullOrWhiteSpace(bankAccount))
      return null;

    var trimmed = bankAccount.Trim();
    if (trimmed.Length <= 4)
      return $"***{trimmed}";

    return $"***{trimmed[^4..]}";
  }

  public static EmploymentProfileDto MapProfile(EmploymentProfile? profile, bool canManage)
  {
    if (profile is null)
    {
      return new EmploymentProfileDto
      {
        CanManage = canManage,
      };
    }

    var bankAccount = string.IsNullOrWhiteSpace(profile.BankAccount) ? null : profile.BankAccount;
    return new EmploymentProfileDto
    {
      EmployeeId = string.IsNullOrWhiteSpace(profile.EmployeeId) ? null : profile.EmployeeId,
      NationalId = string.IsNullOrWhiteSpace(profile.NationalId) ? null : profile.NationalId,
      TaxId = string.IsNullOrWhiteSpace(profile.TaxId) ? null : profile.TaxId,
      BankAccount = canManage ? bankAccount : MaskBankAccount(bankAccount),
      Notes = string.IsNullOrWhiteSpace(profile.Notes) ? null : profile.Notes,
      CanManage = canManage,
    };
  }

  public static EmploymentContractListItemDto MapContractListItem(
    EmploymentContract contract,
    string positionName,
    DateOnly today)
  {
    return new EmploymentContractListItemDto
    {
      Id = contract.Id,
      PositionName = positionName,
      ContractType = contract.ContractType,
      StartDate = contract.StartDate,
      EndDate = contract.EndDate,
      Fte = contract.Fte,
      MonthlyHoursNormMinutes = contract.MonthlyHoursNormMinutes,
      RateOrSalaryDisplay = FormatRateOrSalary(contract.HourlyRate, contract.MonthlySalary),
      Status = GetContractStatus(contract.StartDate, contract.EndDate, today),
    };
  }

  public static async Task<string> GetUserDisplayNameAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var user = await context.Users
      .AsNoTracking()
      .Where(u => u.Id == userId)
      .Select(u => new { u.FirstName, u.LastName, u.Email })
      .FirstOrDefaultAsync(cancellationToken);

    if (user is null)
      return string.Empty;

    var name = $"{user.FirstName} {user.LastName}".Trim();
    return string.IsNullOrWhiteSpace(name) ? user.Email : $"{name} ({user.Email})";
  }
}
