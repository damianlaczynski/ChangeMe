using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record GetTimeSettingsQuery() : IQuery<TimeSettingsDto>;

public class GetTimeSettingsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetTimeSettingsQuery, TimeSettingsDto>
{
  public async Task<Result<TimeSettingsDto>> Handle(
    GetTimeSettingsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.TimeViewReports);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var backdatingLimit = await TimeUtils.GetBackdatingLimitDaysAsync(context, cancellationToken);

    return Result.Success(new TimeSettingsDto
    {
      BackdatingLimitDays = backdatingLimit,
      CanEdit = userAccessor.HasPermission(PermissionCodes.RolesManage),
    });
  }
}
