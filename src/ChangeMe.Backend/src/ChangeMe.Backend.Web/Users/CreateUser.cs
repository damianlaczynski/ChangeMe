using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class CreateUser(IMediator mediator) : BaseEndpoint<CreateUserCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermissions(PermissionCodes.UsersManage, PermissionCodes.RolesManage);
    Post("/users");
    Summary(s => s.Summary = "Create user");
  }
}

public sealed class CreateUserCommandValidator : Validator<CreateUserCommand>
{
  public CreateUserCommandValidator()
  {
    RuleFor(x => x.FirstName)
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

    RuleFor(x => x.LastName)
      .MaximumLength(UserConstraints.NAME_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.LastName));

    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);

    RuleFor(x => x.RoleIds).NotEmpty();
  }
}