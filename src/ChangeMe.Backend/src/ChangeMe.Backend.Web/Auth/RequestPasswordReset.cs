using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class RequestPasswordReset(IMediator mediator)
  : BaseEndpoint<RequestPasswordResetCommand, PasswordResetAckDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/forgot-password");
    AllowAnonymous();
    Summary(s => s.Summary = "Request password reset");
  }
}

public sealed class RequestPasswordResetCommandValidator : Validator<RequestPasswordResetCommand>
{
  public RequestPasswordResetCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);
  }
}
