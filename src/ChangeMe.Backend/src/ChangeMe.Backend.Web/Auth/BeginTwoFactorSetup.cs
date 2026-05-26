using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class BeginTwoFactorSetup(IMediator mediator)
  : BaseEndpoint<BeginTwoFactorSetupCommand, BeginTwoFactorSetupResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/two-factor/setup/begin");
    Summary(s =>
    {
      s.Summary = "Begin two-factor authentication setup";
      s.Description = "Generates a TOTP shared secret and provisioning URI for authenticator apps.";
    });
  }
}

public sealed class BeginTwoFactorSetupCommandValidator : Validator<BeginTwoFactorSetupCommand>
{
  public BeginTwoFactorSetupCommandValidator()
  {
    RuleFor(x => x.CurrentPassword)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.CurrentPassword));
  }
}
