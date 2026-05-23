using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.Web.Validation;

namespace ChangeMe.Backend.Web.Auth;

public class Register(IMediator mediator) : BaseEndpoint<RegisterUserCommand, RegisterUserResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/register");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Register user";
      s.Description =
        "Create a new user account. Returns a session when email verification is disabled; otherwise requires verification first.";
    });
  }
}

public sealed class RegisterUserCommandValidator : Validator<RegisterUserCommand>
{
  public RegisterUserCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);

    RuleFor(x => x.Password)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}