using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class LeaveRequestPermissions
{
  public static LeaveRequestDetailsContext BuildDetailsContext(
    LeaveRequest request,
    IUserAccessor userAccessor,
    string userDisplayName,
    string leaveTypeName,
    string? decidedByDisplayName)
  {
    var isOwner = userAccessor.UserId == request.UserId;
    var canManageLeave = userAccessor.HasPermission(PermissionCodes.BillingManageLeave);
    var canApproveLeave = userAccessor.HasPermission(PermissionCodes.BillingApproveLeave);
    var canViewOwn = userAccessor.HasPermission(PermissionCodes.BillingViewOwn);

    var canEdit =
      (request.Status == LeaveRequestStatus.Draft && ((isOwner && canViewOwn) || canManageLeave))
      || (request.Status == LeaveRequestStatus.Submitted && canManageLeave);

    var canSubmit = request.Status == LeaveRequestStatus.Draft && ((isOwner && canViewOwn) || canManageLeave);

    var canApprove = request.Status == LeaveRequestStatus.Submitted
                     && canApproveLeave
                     && userAccessor.UserId != request.UserId;

    var canReject = canApprove;

    var canCancel = request.Status != LeaveRequestStatus.Cancelled && (
      (isOwner && canViewOwn && request.Status is LeaveRequestStatus.Draft or LeaveRequestStatus.Submitted)
      || canManageLeave);

    var canDelete = request.Status == LeaveRequestStatus.Draft && ((isOwner && canViewOwn) || canManageLeave);

    return new LeaveRequestDetailsContext
    {
      UserDisplayName = userDisplayName,
      LeaveTypeName = leaveTypeName,
      DecidedByDisplayName = decidedByDisplayName,
      CanEdit = canEdit,
      CanSubmit = canSubmit,
      CanApprove = canApprove,
      CanReject = canReject,
      CanCancel = canCancel,
      CanDelete = canDelete,
    };
  }
}

internal static class LeaveRequestWorkflow
{
  public static async Task<Result<LeaveType>> LoadActiveLeaveTypeAsync(
    ApplicationDbContext context,
    Guid leaveTypeId,
    CancellationToken cancellationToken)
  {
    var leaveType = await context.LeaveTypes
      .AsNoTracking()
      .FirstOrDefaultAsync(lt => lt.Id == leaveTypeId, cancellationToken);
    if (leaveType is null)
      return Result.NotFound();

    if (!leaveType.IsActive)
      return Result.Invalid(new ValidationError(nameof(leaveTypeId), "Selected leave type is not active."));

    return Result.Success(leaveType);
  }

  public static async Task<Result<BillingSettings>> LoadBillingSettingsAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken)
  {
    var settings = await context.BillingSettings
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
    return settings is null ? Result.NotFound() : Result.Success(settings);
  }

  public static async Task<Result<LeaveRequest>> PrepareLeaveRequestAsync(
    ApplicationDbContext context,
    Guid userId,
    Guid leaveTypeId,
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    string? reason,
    Guid? excludeRequestId,
    DateOnly today,
    CancellationToken cancellationToken)
  {
    var userExists = await context.Users.AsNoTracking().AnyAsync(u => u.Id == userId && !u.Deactivated, cancellationToken);
    if (!userExists)
      return Result.NotFound();

    var startDateResult = LeaveRequestsUtils.ValidateStartDateWindow(startDate, today);
    if (!startDateResult.IsSuccess)
      return startDateResult.Map();

    var leaveTypeResult = await LoadActiveLeaveTypeAsync(context, leaveTypeId, cancellationToken);
    if (!leaveTypeResult.IsSuccess)
      return leaveTypeResult.Map();

    var settingsResult = await LoadBillingSettingsAsync(context, cancellationToken);
    if (!settingsResult.IsSuccess)
      return settingsResult.Map();

    var resolvedDayPortion = LeaveRequestsUtils.ResolveDayPortion(
      startDate,
      endDate,
      dayPortion,
      settingsResult.Value.AllowHalfDayLeave);

    var overlapResult = await LeaveRequestsUtils.EnsureNoLeaveOverlapAsync(
      context,
      userId,
      startDate,
      endDate,
      excludeRequestId,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    return LeaveRequest.CreateDraft(
      userId,
      leaveTypeId,
      startDate,
      endDate,
      resolvedDayPortion,
      reason);
  }
}
