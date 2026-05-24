using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class BeginExternalSignIn(IMediator mediator)
  : BaseEndpoint<BeginExternalSignInCommand, BeginExternalSignInResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/external/{ProviderKey}/begin");
    AllowAnonymous();
    Summary(s => s.Summary = "Begin external provider sign-in");
  }
}

public sealed class BeginExternalSignInCommandValidator : Validator<BeginExternalSignInCommand>
{
  public BeginExternalSignInCommandValidator()
  {
    RuleFor(x => x.ProviderKey)
      .NotEmpty()
      .MaximumLength(TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH);
  }
}
