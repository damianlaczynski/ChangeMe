using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class BeginExternalStepUp(IMediator mediator)
  : BaseEndpoint<BeginExternalStepUpCommand, BeginExternalSignInResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/external/{ProviderKey}/step-up/begin");
    Summary(s => s.Summary = "Begin external provider step-up authentication");
  }
}

public sealed class BeginExternalStepUpCommandValidator : Validator<BeginExternalStepUpCommand>
{
  public BeginExternalStepUpCommandValidator()
  {
    RuleFor(x => x.ProviderKey)
      .NotEmpty()
      .MaximumLength(TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH);
  }
}
