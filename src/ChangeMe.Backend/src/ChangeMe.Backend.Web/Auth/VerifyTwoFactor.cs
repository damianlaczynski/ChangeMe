using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class VerifyTwoFactor(IMediator mediator)
  : BaseEndpoint<VerifyTwoFactorCommand, AuthResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/two-factor/verify");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Verify two-factor authentication during sign-in";
      s.Description = "Completes sign-in after primary authentication when two-factor is enabled.";
    });
  }
}

public sealed class VerifyTwoFactorCommandValidator : Validator<VerifyTwoFactorCommand>
{
  public VerifyTwoFactorCommandValidator()
  {
    RuleFor(x => x.ChallengeId)
      .NotEmpty();

    RuleFor(x => x.VerificationCode)
      .NotEmpty()
      .MaximumLength(64);
  }
}
