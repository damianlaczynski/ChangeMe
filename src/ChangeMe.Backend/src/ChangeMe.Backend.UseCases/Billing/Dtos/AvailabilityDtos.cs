using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class AvailabilityEntryDto
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public DateOnly StartDate { get; set; }
  public DateOnly EndDate { get; set; }
  public bool AllDay { get; set; }
  public TimeOnly? StartTime { get; set; }
  public TimeOnly? EndTime { get; set; }
  public AvailabilityStatus Status { get; set; }
  public string Notes { get; set; } = string.Empty;
  public AvailabilityEntrySource Source { get; set; }
  public Guid? LeaveRequestId { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}

public class WeeklyRecurringPatternDayDto
{
  public DayOfWeek DayOfWeek { get; set; }
  public bool Enabled { get; set; }
  public TimeOnly? StartTime { get; set; }
  public TimeOnly? EndTime { get; set; }
  public AvailabilityStatus? Status { get; set; }
}

public class WeeklyRecurringPatternDto
{
  public Guid UserId { get; set; }
  public IReadOnlyList<WeeklyRecurringPatternDayDto> Days { get; set; } = [];
  public bool CanEdit { get; set; }
}

public class SaveWeeklyRecurringPatternRequest
{
  public IReadOnlyList<WeeklyRecurringPatternDayDto> Days { get; set; } = [];
}

public class AvailabilityCalendarUserDto
{
  public Guid Id { get; set; }
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
}

public class AvailabilityCalendarResultDto
{
  public DateOnly From { get; set; }
  public DateOnly To { get; set; }
  public IReadOnlyList<AvailabilityCalendarUserDto> Users { get; set; } = [];
  public IReadOnlyList<AvailabilityEntryDto> Entries { get; set; } = [];
  public bool IsTruncated { get; set; }
}

public class CreateAvailabilityEntryRequest
{
  public Guid? UserId { get; set; }
  public DateOnly StartDate { get; set; }
  public DateOnly EndDate { get; set; }
  public bool AllDay { get; set; } = true;
  public TimeOnly? StartTime { get; set; }
  public TimeOnly? EndTime { get; set; }
  public AvailabilityStatus Status { get; set; }
  public string? Notes { get; set; }
}

public class UpdateAvailabilityEntryRequest
{
  public DateOnly StartDate { get; set; }
  public DateOnly EndDate { get; set; }
  public bool AllDay { get; set; } = true;
  public TimeOnly? StartTime { get; set; }
  public TimeOnly? EndTime { get; set; }
  public AvailabilityStatus Status { get; set; }
  public string? Notes { get; set; }
}

public class AvailabilityDayResultDto
{
  public DateOnly Date { get; set; }
  public Guid UserId { get; set; }
  public IReadOnlyList<AvailabilityEntryDto> Entries { get; set; } = [];
  public bool CanManage { get; set; }
}
