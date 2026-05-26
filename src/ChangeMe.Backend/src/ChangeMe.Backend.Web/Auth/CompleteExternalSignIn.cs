using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class CompleteExternalSignIn(IMediator mediator)
  : BaseEndpoint<CompleteExternalSignInCommand, ExternalSignInResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/external/complete");
    AllowAnonymous();
    Summary(s => s.Summary = "Complete external provider sign-in");
  }
}

public sealed class CompleteExternalSignInCommandValidator : Validator<CompleteExternalSignInCommand>
{
  public CompleteExternalSignInCommandValidator()
  {
    RuleFor(x => x.Code).NotEmpty();
    RuleFor(x => x.State).NotEmpty().MaximumLength(128);
  }
}
