namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class BillingSettings : Entity, IAggregateRoot
{
  public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000002");

  private BillingSettings() { }

  public decimal DefaultAnnualLeaveDays { get; private set; } = 26.0m;
  public bool AllowHalfDayLeave { get; private set; } = true;
  public TimeOnly DefaultWorkdayStart { get; private set; } = new(9, 0);
  public TimeOnly DefaultWorkdayEnd { get; private set; } = new(17, 0);
  public TimeOnly HalfDaySplitTime { get; private set; } = new(13, 0);
  public string DefaultWorkdaysCsv { get; private set; } = "1,2,3,4,5";
  public Enums.AvailabilityStatus DefaultAvailabilityStatus { get; private set; } = Enums.AvailabilityStatus.OnSite;

  public static BillingSettings CreateDefault() =>
    new()
    {
      Id = SingletonId,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

  public Result<BillingSettings> Update(
    decimal defaultAnnualLeaveDays,
    bool allowHalfDayLeave,
    TimeOnly defaultWorkdayStart,
    TimeOnly defaultWorkdayEnd,
    TimeOnly halfDaySplitTime,
    IReadOnlyList<DayOfWeek> defaultWorkdays,
    Enums.AvailabilityStatus defaultAvailabilityStatus)
  {
    var validationErrors = new List<ValidationError>();

    if (defaultAnnualLeaveDays < BillingConstraints.MinAnnualLeaveDays
        || defaultAnnualLeaveDays > BillingConstraints.MaxAnnualLeaveDays
        || decimal.Round(defaultAnnualLeaveDays, 1) != defaultAnnualLeaveDays)
    {
      validationErrors.Add(new ValidationError(
        nameof(DefaultAnnualLeaveDays),
        BillingConstraints.AnnualLeaveDaysRangeMessage));
    }

    if (defaultWorkdayEnd <= defaultWorkdayStart)
    {
      validationErrors.Add(new ValidationError(
        nameof(DefaultWorkdayEnd),
        BillingConstraints.DefaultWorkdayEndBeforeStartMessage));
    }

    if (halfDaySplitTime <= defaultWorkdayStart || halfDaySplitTime >= defaultWorkdayEnd)
    {
      validationErrors.Add(new ValidationError(
        nameof(HalfDaySplitTime),
        BillingConstraints.HalfDaySplitOutOfRangeMessage));
    }

    if (defaultWorkdays.Count == 0)
    {
      validationErrors.Add(new ValidationError(
        nameof(DefaultWorkdaysCsv),
        BillingConstraints.DefaultWorkdaysRequiredMessage));
    }

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    DefaultAnnualLeaveDays = defaultAnnualLeaveDays;
    AllowHalfDayLeave = allowHalfDayLeave;
    DefaultWorkdayStart = defaultWorkdayStart;
    DefaultWorkdayEnd = defaultWorkdayEnd;
    HalfDaySplitTime = halfDaySplitTime;
    DefaultWorkdaysCsv = SerializeWorkdays(defaultWorkdays);
    DefaultAvailabilityStatus = defaultAvailabilityStatus;

    return Result.Success(this);
  }

  public IReadOnlyList<DayOfWeek> GetDefaultWorkdays() => ParseWorkdays(DefaultWorkdaysCsv);

  public static string SerializeWorkdays(IEnumerable<DayOfWeek> workdays) =>
    string.Join(',', workdays.Select(d => ((int)d).ToString()));

  public static IReadOnlyList<DayOfWeek> ParseWorkdays(string csv) =>
    string.IsNullOrWhiteSpace(csv)
      ? []
      : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(value => (DayOfWeek)int.Parse(value))
        .Distinct()
        .OrderBy(day => day)
        .ToList();
}
