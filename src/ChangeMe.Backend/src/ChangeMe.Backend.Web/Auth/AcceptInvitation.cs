using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.Web.Validation;

namespace ChangeMe.Backend.Web.Auth;

public class AcceptInvitation(IMediator mediator) : BaseEndpoint<AcceptInvitationCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/accept-invitation");
    AllowAnonymous();
    Summary(s => s.Summary = "Accept invitation");
  }
}

public sealed class AcceptInvitationCommandValidator : Validator<AcceptInvitationCommand>
{
  public AcceptInvitationCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.Token).NotEmpty();

    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Password)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}
