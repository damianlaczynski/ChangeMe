using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class UpdateTimeSettings(IMediator mediator) : BaseEndpoint<UpdateTimeSettingsCommand, TimeSettingsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/time/settings");
    RequirePermission(PermissionCodes.RolesManage);
    Summary(s => s.Summary = "Update time tracking settings");
  }
}

public sealed class UpdateTimeSettingsCommandValidator : Validator<UpdateTimeSettingsCommand>
{
  public UpdateTimeSettingsCommandValidator()
  {
    RuleFor(x => x.BackdatingLimitDays)
      .InclusiveBetween(TimeConstraints.MinBackdatingLimitDays, TimeConstraints.MaxBackdatingLimitDays);
  }
}
