using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Billing.Entities;

public class WeeklyRecurringPattern : Entity, IAggregateRoot
{
  private readonly List<WeeklyRecurringPatternDay> days = [];

  private WeeklyRecurringPattern() { }

  public Guid UserId { get; private set; }

  public IReadOnlyCollection<WeeklyRecurringPatternDay> Days => days.AsReadOnly();

  public static WeeklyRecurringPattern CreateEmpty(Guid userId) =>
    new()
    {
      UserId = userId,
    };

  public static WeeklyRecurringPattern CreateDefault(
    Guid userId,
    BillingSettings settings,
    decimal fte)
  {
    var pattern = CreateEmpty(userId);
    foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
    {
      var enabled = settings.GetDefaultWorkdays().Contains(day);
      pattern.days.Add(WeeklyRecurringPatternDay.Create(
        day,
        enabled,
        enabled ? settings.DefaultWorkdayStart : null,
        enabled ? ScaleEndTime(settings, fte) : null,
        enabled ? settings.DefaultAvailabilityStatus : null));
    }

    return pattern;
  }

  public Result ReplaceDays(IReadOnlyList<WeeklyRecurringPatternDayInput> dayInputs)
  {
    var validationErrors = new List<ValidationError>();
    foreach (var dayInput in dayInputs)
    {
      if (dayInput.Enabled)
      {
        if (dayInput.StartTime is null || dayInput.EndTime is null)
        {
          validationErrors.Add(new ValidationError(
            nameof(WeeklyRecurringPatternDay.StartTime),
            "Enter start and end times."));
        }
        else if (dayInput.EndTime <= dayInput.StartTime)
        {
          validationErrors.Add(new ValidationError(
            nameof(WeeklyRecurringPatternDay.EndTime),
            "End time must be after start time."));
        }

        if (dayInput.Status is AvailabilityStatus.Unavailable)
        {
          validationErrors.Add(new ValidationError(
            nameof(WeeklyRecurringPatternDay.Status),
            "Unavailable is not allowed on recurring rows."));
        }
      }
    }

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    days.Clear();
    foreach (var dayInput in dayInputs.OrderBy(d => d.DayOfWeek))
    {
      days.Add(WeeklyRecurringPatternDay.Create(
        dayInput.DayOfWeek,
        dayInput.Enabled,
        dayInput.StartTime,
        dayInput.EndTime,
        dayInput.Status));
    }

    return Result.Success();
  }

  public Result ApplyOrganizationDefaults(BillingSettings settings, decimal fte)
  {
    var dayInputs = Enum.GetValues<DayOfWeek>()
      .Select(day =>
      {
        var enabled = settings.GetDefaultWorkdays().Contains(day);
        return new WeeklyRecurringPatternDayInput(
          day,
          enabled,
          enabled ? settings.DefaultWorkdayStart : null,
          enabled ? ScaleEndTime(settings, fte) : null,
          enabled ? settings.DefaultAvailabilityStatus : null);
      })
      .ToList();

    return ReplaceDays(dayInputs);
  }

  private static TimeOnly ScaleEndTime(BillingSettings settings, decimal fte)
  {
    if (fte >= 1m)
      return settings.DefaultWorkdayEnd;

    var durationMinutes = (int)(settings.DefaultWorkdayEnd - settings.DefaultWorkdayStart).TotalMinutes;
    var scaledMinutes = (int)(durationMinutes * fte);
    scaledMinutes = scaledMinutes / 15 * 15;
    return settings.DefaultWorkdayStart.AddMinutes(Math.Max(scaledMinutes, 15));
  }
}

public record WeeklyRecurringPatternDayInput(
  DayOfWeek DayOfWeek,
  bool Enabled,
  TimeOnly? StartTime,
  TimeOnly? EndTime,
  AvailabilityStatus? Status);

public class WeeklyRecurringPatternDay : Entity
{
  private WeeklyRecurringPatternDay() { }

  public Guid PatternId { get; set; }
  public DayOfWeek DayOfWeek { get; private set; }
  public bool Enabled { get; private set; }
  public TimeOnly? StartTime { get; private set; }
  public TimeOnly? EndTime { get; private set; }
  public AvailabilityStatus? Status { get; private set; }

  public static WeeklyRecurringPatternDay Create(
    DayOfWeek dayOfWeek,
    bool enabled,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityStatus? status) =>
    new()
    {
      DayOfWeek = dayOfWeek,
      Enabled = enabled,
      StartTime = startTime,
      EndTime = endTime,
      Status = status,
    };
}
