using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class ConfirmTwoFactorSetup(IMediator mediator)
  : BaseEndpoint<ConfirmTwoFactorSetupCommand, TwoFactorSetupCompletedDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/two-factor/setup/confirm");
    Summary(s =>
    {
      s.Summary = "Confirm two-factor authentication setup";
      s.Description = "Validates the first TOTP code and enables two-factor authentication.";
    });
  }
}

public sealed class ConfirmTwoFactorSetupCommandValidator : Validator<ConfirmTwoFactorSetupCommand>
{
  public ConfirmTwoFactorSetupCommandValidator()
  {
    RuleFor(x => x.VerificationCode)
      .NotEmpty()
      .MaximumLength(64);
  }
}
