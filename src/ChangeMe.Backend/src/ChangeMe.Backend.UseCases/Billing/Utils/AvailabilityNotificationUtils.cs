namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class AvailabilityNotificationUtils
{
  public static string FormatDateRange(DateOnly startDate, DateOnly endDate) =>
    startDate == endDate
      ? startDate.ToString("dd.MM.yyyy")
      : $"{startDate:dd.MM.yyyy} – {endDate:dd.MM.yyyy}";

  public static string FormatTimeRange(bool allDay, TimeOnly? startTime, TimeOnly? endTime) =>
    allDay
      ? "All day"
      : $"{startTime:HH\\:mm}–{endTime:HH\\:mm}";

  public static string BuildMyAvailabilityLink(DateOnly? startDate, DateOnly? endDate)
  {
    if (startDate.HasValue && (!endDate.HasValue || endDate == startDate))
      return $"/my-availability?date={startDate.Value:yyyy-MM-dd}";

    return "/my-availability";
  }
}
