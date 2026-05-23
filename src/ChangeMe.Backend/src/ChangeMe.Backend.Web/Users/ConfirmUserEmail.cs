using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class ConfirmUserEmail(IMediator mediator) : BaseEndpoint<ConfirmUserEmailCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/confirm-email");
    Summary(s => s.Summary = "Mark user email as verified");
  }
}

public sealed class ConfirmUserEmailCommandValidator : Validator<ConfirmUserEmailCommand>
{
  public ConfirmUserEmailCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
