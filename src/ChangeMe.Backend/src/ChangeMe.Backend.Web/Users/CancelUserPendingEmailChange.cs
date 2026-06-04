using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class CancelUserPendingEmailChange(IMediator mediator)
  : BaseEndpoint<CancelUserPendingEmailChangeCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/cancel-pending-email-change");
    Summary(s => s.Summary = "Cancel a user's pending self-service email change");
  }
}

public sealed class CancelUserPendingEmailChangeCommandValidator
  : Validator<CancelUserPendingEmailChangeCommand>
{
  public CancelUserPendingEmailChangeCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
