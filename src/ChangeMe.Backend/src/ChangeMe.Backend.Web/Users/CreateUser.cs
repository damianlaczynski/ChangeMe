using ChangeMe.Backend.Domain.Aggregates.Users;
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
    RuleFor(x => x.FirstName).NotEmpty().MaximumLength(UserConstraints.NAME_MAX_LENGTH);
    RuleFor(x => x.LastName).NotEmpty().MaximumLength(UserConstraints.NAME_MAX_LENGTH);
    RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);
    RuleFor(x => x.Password)
      .NotEmpty()
      .MinimumLength(UserConstraints.PASSWORD_MIN_LENGTH)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH);
    RuleFor(x => x.RoleIds).NotEmpty();
  }
}
