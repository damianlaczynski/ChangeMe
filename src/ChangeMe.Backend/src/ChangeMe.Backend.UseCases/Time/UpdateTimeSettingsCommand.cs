using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record UpdateTimeSettingsCommand(int BackdatingLimitDays) : ICommand<TimeSettingsDto>;

public class UpdateTimeSettingsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateTimeSettingsCommand, TimeSettingsDto>
{
  public async Task<Result<TimeSettingsDto>> Handle(
    UpdateTimeSettingsCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.RolesManage);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var settings = await context.TimeTrackingSettings
      .FirstOrDefaultAsync(x => x.Id == TimeTrackingSettings.SingletonId, cancellationToken);

    if (settings is null)
    {
      settings = TimeTrackingSettings.CreateDefault();
      await context.TimeTrackingSettings.AddAsync(settings, cancellationToken);
    }

    var updateResult = settings.UpdateBackdatingLimitDays(command.BackdatingLimitDays);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new TimeSettingsDto
    {
      BackdatingLimitDays = settings.BackdatingLimitDays,
      CanEdit = true,
    });
  }
}
