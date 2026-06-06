namespace ChangeMe.Backend.Domain.Aggregates.Time;

public static class TimeConstraints
{
  public const int DescriptionMaxLength = 500;
  public const int MinDurationMinutes = 1;
  public const int MaxDurationMinutes = 1440;
  public const int DefaultBackdatingLimitDays = 30;
  public const int MinBackdatingLimitDays = 0;
  public const int MaxBackdatingLimitDays = 3650;

  public const string ProjectRequiredMessage = "Project is required.";
  public const string WorkDateOutsideRangeMessage = "Work date is outside the allowed range.";
  public const string DurationRangeMessage = "Duration must be between 1 minute and 24 hours.";
  public const string DescriptionTooLongMessage = "Description cannot exceed 500 characters.";
  public const string IssueMustBelongToProjectMessage = "Issue must belong to the selected project.";
  public const string BackdatingLimitInvalidMessage = "Enter a whole number of days from 0 to 3650.";
  public const string TimerAlreadyRunningMessage = "You already have a running timer.";
  public const string TimerNotRunningMessage = "No running timer.";
  public const string TimerMinimumDurationMessage = "Timer must run at least 1 minute before logging.";
}
