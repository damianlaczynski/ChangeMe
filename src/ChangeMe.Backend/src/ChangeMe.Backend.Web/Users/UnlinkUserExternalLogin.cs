using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class UnlinkUserExternalLogin(IMediator mediator)
  : BaseEndpoint<UnlinkUserExternalLoginCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{UserId}/external-logins/{ProviderKey}/unlink");
    Summary(s => s.Summary = "Unlink an external provider from a user account");
  }
}

public sealed class UnlinkUserExternalLoginCommandValidator : Validator<UnlinkUserExternalLoginCommand>
{
  public UnlinkUserExternalLoginCommandValidator()
  {
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.ProviderKey)
      .NotEmpty()
      .MaximumLength(TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH);
  }
}
