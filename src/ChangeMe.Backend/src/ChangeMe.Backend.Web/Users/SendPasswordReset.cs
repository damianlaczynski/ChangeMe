using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class SendPasswordReset(IMediator mediator) : BaseEndpoint<SendPasswordResetCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/send-password-reset");
    Summary(s => s.Summary = "Send password reset email");
  }
}

public sealed class SendPasswordResetCommandValidator : Validator<SendPasswordResetCommand>
{
  public SendPasswordResetCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
