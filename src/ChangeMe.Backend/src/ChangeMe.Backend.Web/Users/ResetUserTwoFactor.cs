using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class ResetUserTwoFactor(IMediator mediator)
  : BaseEndpoint<ResetUserTwoFactorCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/reset-two-factor");
    Summary(s => s.Summary = "Reset two-factor authentication for a user");
  }
}

public sealed class ResetUserTwoFactorCommandValidator : Validator<ResetUserTwoFactorCommand>
{
  public ResetUserTwoFactorCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
