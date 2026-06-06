using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Billing.Entities;

public class AvailabilityEntry : Entity
{
  private AvailabilityEntry() { }

  public Guid UserId { get; private set; }
  public DateOnly StartDate { get; private set; }
  public DateOnly EndDate { get; private set; }
  public bool AllDay { get; private set; } = true;
  public TimeOnly? StartTime { get; private set; }
  public TimeOnly? EndTime { get; private set; }
  public AvailabilityStatus Status { get; private set; }
  public string Notes { get; private set; } = string.Empty;
  public AvailabilityEntrySource Source { get; private set; }
  public Guid? LeaveRequestId { get; private set; }

  public static Result<AvailabilityEntry> CreateManual(
    Guid userId,
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityStatus status,
    string? notes) =>
    Create(
      userId,
      startDate,
      endDate,
      allDay,
      startTime,
      endTime,
      status,
      notes,
      AvailabilityEntrySource.Manual,
      leaveRequestId: null);

  public static Result<AvailabilityEntry> CreateRecurring(
    Guid userId,
    DateOnly date,
    TimeOnly startTime,
    TimeOnly endTime,
    AvailabilityStatus status) =>
    Create(
      userId,
      date,
      date,
      allDay: false,
      startTime,
      endTime,
      status,
      notes: null,
      AvailabilityEntrySource.Recurring,
      leaveRequestId: null);

  public static Result<AvailabilityEntry> CreateLeave(
    Guid userId,
    DateOnly date,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    string leaveTypeName,
    Guid leaveRequestId) =>
    Create(
      userId,
      date,
      date,
      allDay,
      startTime,
      endTime,
      AvailabilityStatus.Unavailable,
      leaveTypeName,
      AvailabilityEntrySource.Leave,
      leaveRequestId);

  public Result<AvailabilityEntry> UpdateManual(
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityStatus status,
    string? notes)
  {
    if (Source != AvailabilityEntrySource.Manual)
      return Result.Conflict("Only manual availability entries can be edited.");

    var validationErrors = Validate(userId: UserId, startDate, endDate, allDay, startTime, endTime, notes);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    StartDate = startDate;
    EndDate = endDate;
    AllDay = allDay;
    StartTime = allDay ? null : startTime;
    EndTime = allDay ? null : endTime;
    Status = status;
    Notes = notes?.Trim() ?? string.Empty;

    return Result.Success(this);
  }

  private static Result<AvailabilityEntry> Create(
    Guid userId,
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityStatus status,
    string? notes,
    AvailabilityEntrySource source,
    Guid? leaveRequestId)
  {
    var validationErrors = Validate(userId, startDate, endDate, allDay, startTime, endTime, notes);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    if (source == AvailabilityEntrySource.Leave && leaveRequestId is null)
    {
      return Result.Invalid(new ValidationError(
        nameof(LeaveRequestId),
        "Leave request is required for leave availability entries."));
    }

    return Result.Success(new AvailabilityEntry
    {
      UserId = userId,
      StartDate = startDate,
      EndDate = endDate,
      AllDay = allDay,
      StartTime = allDay ? null : startTime,
      EndTime = allDay ? null : endTime,
      Status = status,
      Notes = notes?.Trim() ?? string.Empty,
      Source = source,
      LeaveRequestId = leaveRequestId,
    });
  }

  private static List<ValidationError> Validate(
    Guid userId,
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    string? notes)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "User is required."));

    if (endDate < startDate)
      validationErrors.Add(new ValidationError(nameof(EndDate), "End date must be on or after start date."));

    if (!allDay)
    {
      if (startDate != endDate)
        validationErrors.Add(new ValidationError(nameof(AllDay), "Timed entries must be single-day."));

      if (!startTime.HasValue || !endTime.HasValue || endTime <= startTime)
        validationErrors.Add(new ValidationError(nameof(EndTime), "End time must be after start time."));
    }

    if (notes is not null && notes.Trim().Length > BillingConstraints.AvailabilityNotesMaxLength)
    {
      validationErrors.Add(new ValidationError(
        nameof(Notes),
        $"cannot be longer than {BillingConstraints.AvailabilityNotesMaxLength} characters"));
    }

    return validationErrors;
  }
}
