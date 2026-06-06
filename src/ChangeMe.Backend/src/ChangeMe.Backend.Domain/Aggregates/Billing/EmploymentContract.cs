using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class EmploymentContract : Entity, IAggregateRoot
{
  private EmploymentContract() { }

  public Guid UserId { get; private set; }
  public Guid PositionId { get; private set; }
  public ContractType ContractType { get; private set; }
  public DateOnly StartDate { get; private set; }
  public DateOnly? EndDate { get; private set; }
  public decimal Fte { get; private set; }
  public int MonthlyHoursNormMinutes { get; private set; }
  public decimal? HourlyRate { get; private set; }
  public decimal? MonthlySalary { get; private set; }
  public string Notes { get; private set; } = string.Empty;

  public static Result<EmploymentContract> Create(
    Guid userId,
    Guid positionId,
    ContractType contractType,
    DateOnly startDate,
    DateOnly? endDate,
    decimal fte,
    int monthlyHoursNormMinutes,
    decimal? hourlyRate,
    decimal? monthlySalary,
    string? notes)
  {
    var validationErrors = Validate(
      userId,
      positionId,
      startDate,
      endDate,
      fte,
      monthlyHoursNormMinutes,
      hourlyRate,
      monthlySalary,
      notes);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var contract = new EmploymentContract
    {
      UserId = userId,
      PositionId = positionId,
      ContractType = contractType,
      StartDate = startDate,
      EndDate = endDate,
      Fte = decimal.Round(fte, 2),
      MonthlyHoursNormMinutes = monthlyHoursNormMinutes,
      HourlyRate = hourlyRate.HasValue ? decimal.Round(hourlyRate.Value, 2) : null,
      MonthlySalary = monthlySalary.HasValue ? decimal.Round(monthlySalary.Value, 2) : null,
      Notes = notes?.Trim() ?? string.Empty,
    };

    return Result.Success(contract);
  }

  public Result<EmploymentContract> Update(
    Guid userId,
    Guid positionId,
    ContractType contractType,
    DateOnly startDate,
    DateOnly? endDate,
    decimal fte,
    int monthlyHoursNormMinutes,
    decimal? hourlyRate,
    decimal? monthlySalary,
    string? notes)
  {
    var validationErrors = Validate(
      userId,
      positionId,
      startDate,
      endDate,
      fte,
      monthlyHoursNormMinutes,
      hourlyRate,
      monthlySalary,
      notes);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    UserId = userId;
    PositionId = positionId;
    ContractType = contractType;
    StartDate = startDate;
    EndDate = endDate;
    Fte = decimal.Round(fte, 2);
    MonthlyHoursNormMinutes = monthlyHoursNormMinutes;
    HourlyRate = hourlyRate.HasValue ? decimal.Round(hourlyRate.Value, 2) : null;
    MonthlySalary = monthlySalary.HasValue ? decimal.Round(monthlySalary.Value, 2) : null;
    Notes = notes?.Trim() ?? string.Empty;

    return Result.Success(this);
  }

  public static bool Overlaps(
    DateOnly startDate,
    DateOnly? endDate,
    DateOnly otherStartDate,
    DateOnly? otherEndDate)
  {
    var effectiveEnd = endDate ?? DateOnly.MaxValue;
    var otherEffectiveEnd = otherEndDate ?? DateOnly.MaxValue;
    return startDate <= otherEffectiveEnd && otherStartDate <= effectiveEnd;
  }

  private static List<ValidationError> Validate(
    Guid userId,
    Guid positionId,
    DateOnly startDate,
    DateOnly? endDate,
    decimal fte,
    int monthlyHoursNormMinutes,
    decimal? hourlyRate,
    decimal? monthlySalary,
    string? notes)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "User is required."));

    if (positionId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(PositionId), "Position is required."));

    if (endDate.HasValue && endDate.Value < startDate)
      validationErrors.Add(new ValidationError(nameof(EndDate), "End date must be on or after start date."));

    if (fte < BillingConstraints.MinFte || fte > BillingConstraints.MaxFte || decimal.Round(fte, 2) != fte)
      validationErrors.Add(new ValidationError(nameof(Fte), "FTE must be between 0.01 and 1.00 with two decimal places."));

    if (monthlyHoursNormMinutes < BillingConstraints.MinMonthlyHoursNormMinutes
        || monthlyHoursNormMinutes > BillingConstraints.MaxMonthlyHoursNormMinutes)
    {
      validationErrors.Add(new ValidationError(
        nameof(MonthlyHoursNormMinutes),
        "Monthly hours norm must be between 60 and 10080 minutes."));
    }

    if (hourlyRate.HasValue && hourlyRate.Value < BillingConstraints.MinCompensationAmount)
      validationErrors.Add(new ValidationError(nameof(HourlyRate), "Hourly rate must be at least 0.01 when provided."));

    if (monthlySalary.HasValue && monthlySalary.Value < BillingConstraints.MinCompensationAmount)
      validationErrors.Add(new ValidationError(nameof(MonthlySalary), "Monthly salary must be at least 0.01 when provided."));

    if (!hourlyRate.HasValue && !monthlySalary.HasValue)
      validationErrors.Add(new ValidationError(nameof(HourlyRate), BillingConstraints.ContractCompensationRequiredMessage));

    if (notes is not null && notes.Trim().Length > BillingConstraints.ContractNotesMaxLength)
      validationErrors.Add(new ValidationError(nameof(Notes), $"cannot be longer than {BillingConstraints.ContractNotesMaxLength} characters"));

    return validationErrors;
  }
}
