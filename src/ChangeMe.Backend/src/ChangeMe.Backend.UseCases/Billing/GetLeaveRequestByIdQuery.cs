using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetLeaveRequestByIdQuery(Guid Id) : IQuery<LeaveRequestDetailsDto>;

public class GetLeaveRequestByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetLeaveRequestByIdQuery, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    GetLeaveRequestByIdQuery query,
    CancellationToken cancellationToken)
  {
    var request = await context.LeaveRequests
      .AsNoTracking()
      .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);
    if (request is null)
      return Result.NotFound();

    var accessResult = EnsureCanViewRequest(request, userAccessor);
    if (!accessResult.IsSuccess)
      return accessResult.Map();

    return await MapDetailsAsync(context, userAccessor, request, cancellationToken);
  }

  internal static Result EnsureCanViewRequest(LeaveRequest request, IUserAccessor userAccessor)
  {
    if (userAccessor.UserId == request.UserId
        && userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
    {
      return Result.Success();
    }

    return LeaveRequestsUtils.CanViewLeaveRequests(userAccessor)
      ? Result.Success()
      : Result.Forbidden();
  }

  internal static async Task<Result<LeaveRequestDetailsDto>> MapDetailsAsync(
    ApplicationDbContext context,
    IUserAccessor userAccessor,
    LeaveRequest request,
    CancellationToken cancellationToken)
  {
    var leaveType = await context.LeaveTypes
      .AsNoTracking()
      .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId, cancellationToken);
    if (leaveType is null)
      return Result.NotFound();

    var userDisplayName = await EmploymentUtils.GetUserDisplayNameAsync(context, request.UserId, cancellationToken);
    string? decidedByDisplayName = null;
    if (request.DecidedByUserId.HasValue)
    {
      decidedByDisplayName = await EmploymentUtils.GetUserDisplayNameAsync(
        context,
        request.DecidedByUserId.Value,
        cancellationToken);
    }

    var detailsContext = LeaveRequestPermissions.BuildDetailsContext(
      request,
      userAccessor,
      userDisplayName,
      leaveType.Name,
      decidedByDisplayName);

    return Result.Success(LeaveRequestsUtils.MapDetails(request, detailsContext));
  }
}
