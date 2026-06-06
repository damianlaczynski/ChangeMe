using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class LeaveRequest : Entity, IAggregateRoot
{
  private LeaveRequest() { }

  public Guid UserId { get; private set; }
  public Guid LeaveTypeId { get; private set; }
  public DateOnly StartDate { get; private set; }
  public DateOnly EndDate { get; private set; }
  public LeaveDayPortion? DayPortion { get; private set; }
  public LeaveRequestStatus Status { get; private set; } = LeaveRequestStatus.Draft;
  public DateTime? SubmittedAt { get; private set; }
  public DateTime? DecidedAt { get; private set; }
  public Guid? DecidedByUserId { get; private set; }
  public string Reason { get; private set; } = string.Empty;
  public string RejectReason { get; private set; } = string.Empty;

  public static Result<LeaveRequest> CreateDraft(
    Guid userId,
    Guid leaveTypeId,
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    string? reason)
  {
    var validationErrors = ValidateDatesAndReason(startDate, endDate, dayPortion, reason, rejectReason: null);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    if (userId == Guid.Empty)
      return Result.Invalid(new ValidationError(nameof(UserId), "User is required."));

    if (leaveTypeId == Guid.Empty)
      return Result.Invalid(new ValidationError(nameof(LeaveTypeId), "Leave type is required."));

    return Result.Success(new LeaveRequest
    {
      UserId = userId,
      LeaveTypeId = leaveTypeId,
      StartDate = startDate,
      EndDate = endDate,
      DayPortion = dayPortion,
      Reason = reason?.Trim() ?? string.Empty,
      Status = LeaveRequestStatus.Draft,
    });
  }

  public Result Submit(DateTime submittedAt, bool requiresApproval)
  {
    if (Status != LeaveRequestStatus.Draft)
      return Result.Conflict("Only draft leave requests can be submitted.");

    SubmittedAt = submittedAt;
    if (requiresApproval)
    {
      Status = LeaveRequestStatus.Submitted;
    }
    else
    {
      Status = LeaveRequestStatus.Approved;
      DecidedAt = submittedAt;
      DecidedByUserId = null;
      RejectReason = string.Empty;
    }

    return Result.Success();
  }

  public Result<LeaveRequest> Update(
    Guid leaveTypeId,
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    string? reason)
  {
    if (Status is not LeaveRequestStatus.Draft and not LeaveRequestStatus.Submitted)
      return Result.Conflict("Only draft or submitted leave requests can be edited.");

    var validationErrors = ValidateDatesAndReason(startDate, endDate, dayPortion, reason, rejectReason: null);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    if (leaveTypeId == Guid.Empty)
      return Result.Invalid(new ValidationError(nameof(LeaveTypeId), "Leave type is required."));

    LeaveTypeId = leaveTypeId;
    StartDate = startDate;
    EndDate = endDate;
    DayPortion = dayPortion;
    Reason = reason?.Trim() ?? string.Empty;

    return Result.Success(this);
  }

  public Result Approve(Guid approverUserId, DateTime decidedAt)
  {
    if (Status != LeaveRequestStatus.Submitted)
      return Result.Conflict("Only submitted leave requests can be approved.");

    if (approverUserId == UserId)
      return Result.Forbidden(BillingConstraints.CannotApproveOwnLeaveMessage);

    Status = LeaveRequestStatus.Approved;
    DecidedAt = decidedAt;
    DecidedByUserId = approverUserId;
    RejectReason = string.Empty;
    return Result.Success();
  }

  public Result Reject(Guid approverUserId, string rejectReason, DateTime decidedAt)
  {
    if (Status != LeaveRequestStatus.Submitted)
      return Result.Conflict("Only submitted leave requests can be rejected.");

    if (approverUserId == UserId)
      return Result.Forbidden(BillingConstraints.CannotApproveOwnLeaveMessage);

    var trimmedReason = rejectReason?.Trim() ?? string.Empty;
    if (trimmedReason.Length == 0 || trimmedReason.Length > BillingConstraints.LeaveRejectReasonMaxLength)
    {
      return Result.Invalid(new ValidationError(
        nameof(RejectReason),
        "Reject reason is required and cannot exceed 500 characters."));
    }

    Status = LeaveRequestStatus.Rejected;
    DecidedAt = decidedAt;
    DecidedByUserId = approverUserId;
    RejectReason = trimmedReason;
    return Result.Success();
  }

  public Result Cancel(bool asAdministrator = false)
  {
    if (Status == LeaveRequestStatus.Cancelled)
      return Result.Conflict("Leave request is already cancelled.");

    if (!asAdministrator && Status is LeaveRequestStatus.Approved or LeaveRequestStatus.Rejected)
      return Result.Conflict("Approved or rejected leave requests cannot be cancelled.");

    Status = LeaveRequestStatus.Cancelled;
    return Result.Success();
  }

  private static List<ValidationError> ValidateDatesAndReason(
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    string? reason,
    string? rejectReason)
  {
    var validationErrors = new List<ValidationError>();

    if (endDate < startDate)
      validationErrors.Add(new ValidationError(nameof(EndDate), "End date must be on or after start date."));

    if (startDate == endDate && dayPortion is null)
      validationErrors.Add(new ValidationError(nameof(DayPortion), "Day portion is required for single-day leave."));

    if (startDate != endDate && dayPortion is not null)
      validationErrors.Add(new ValidationError(nameof(DayPortion), "Day portion applies only to single-day leave."));

    if (reason is not null && reason.Trim().Length > BillingConstraints.LeaveReasonMaxLength)
      validationErrors.Add(new ValidationError(nameof(Reason), $"cannot be longer than {BillingConstraints.LeaveReasonMaxLength} characters"));

    if (rejectReason is not null && rejectReason.Trim().Length > BillingConstraints.LeaveRejectReasonMaxLength)
      validationErrors.Add(new ValidationError(nameof(RejectReason), $"cannot be longer than {BillingConstraints.LeaveRejectReasonMaxLength} characters"));

    return validationErrors;
  }
}
