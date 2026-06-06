namespace ChangeMe.Backend.Domain.Aggregates.Time;

public class TimeTrackingSettings : Entity, IAggregateRoot
{
  public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

  private TimeTrackingSettings() { }

  public int BackdatingLimitDays { get; private set; } = TimeConstraints.DefaultBackdatingLimitDays;

  public static TimeTrackingSettings CreateDefault() =>
    new()
    {
      Id = SingletonId,
      BackdatingLimitDays = TimeConstraints.DefaultBackdatingLimitDays,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

  public Result<TimeTrackingSettings> UpdateBackdatingLimitDays(int backdatingLimitDays)
  {
    if (backdatingLimitDays < TimeConstraints.MinBackdatingLimitDays
        || backdatingLimitDays > TimeConstraints.MaxBackdatingLimitDays)
    {
      return Result.Invalid(
        new ValidationError(
          nameof(BackdatingLimitDays),
          TimeConstraints.BackdatingLimitInvalidMessage));
    }

    BackdatingLimitDays = backdatingLimitDays;
    return Result.Success(this);
  }
}
