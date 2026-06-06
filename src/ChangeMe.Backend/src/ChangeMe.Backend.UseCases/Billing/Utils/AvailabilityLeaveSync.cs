using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class AvailabilityLeaveSync
{
  public static async Task SyncIfApprovedAsync(
    ApplicationDbContext context,
    LeaveRequest request,
    CancellationToken cancellationToken)
  {
    if (request.Status != LeaveRequestStatus.Approved)
      return;

    var leaveType = await context.LeaveTypes
      .AsNoTracking()
      .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId, cancellationToken);
    if (leaveType is null)
      return;

    var settings = await context.BillingSettings
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == Domain.Aggregates.Billing.BillingSettings.SingletonId, cancellationToken);

    await AvailabilityUtils.SyncLeaveEntriesAsync(
      context,
      request,
      leaveType.Name,
      settings,
      cancellationToken);
  }

  public static async Task RemoveIfNeededAsync(
    ApplicationDbContext context,
    LeaveRequest request,
    LeaveRequestStatus previousStatus,
    CancellationToken cancellationToken)
  {
    if (previousStatus == LeaveRequestStatus.Approved)
      await AvailabilityUtils.RemoveLeaveEntriesAsync(context, request.Id, cancellationToken);
  }
}
