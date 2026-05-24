using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.Web.Common;

namespace ChangeMe.Backend.Web.Auth;

public class BeginPasskeySignIn(IMediator mediator)
  : BaseEndpoint<BeginPasskeySignInCommand, PasskeyCeremonyBeginResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/sign-in/begin");
    AllowAnonymous();
    Summary(s => s.Summary = "Begin passkey sign-in ceremony");
  }
}

public class CompletePasskeySignIn(IMediator mediator)
  : BaseEndpoint<CompletePasskeySignInCommand, LoginResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/sign-in/complete");
    AllowAnonymous();
    Summary(s => s.Summary = "Complete passkey sign-in ceremony");
  }
}

public class BeginPasskeyRegistration(IMediator mediator)
  : BaseEndpoint<BeginPasskeyRegistrationCommand, PasskeyCeremonyBeginResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/register/begin");
    Summary(s => s.Summary = "Begin passkey registration ceremony");
  }
}

public class CompletePasskeyRegistration(IMediator mediator)
  : BaseEndpoint<CompletePasskeyRegistrationCommand, MyAccountPasskeyDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/register/complete");
    Summary(s => s.Summary = "Complete passkey registration ceremony");
  }
}

public class RenamePasskey(IMediator mediator)
  : BaseEndpoint<RenamePasskeyCommand, MyAccountPasskeyDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/{PasskeyId}/rename");
    Summary(s => s.Summary = "Rename a passkey");
  }
}

public class RemovePasskey(IMediator mediator) : BaseEndpoint<RemovePasskeyCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/{PasskeyId}/remove");
    Summary(s => s.Summary = "Remove a passkey from the current account");
  }
}

public class BeginPasskeyStepUp(IMediator mediator)
  : BaseEndpoint<BeginPasskeyStepUpCommand, PasskeyCeremonyBeginResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/step-up/begin");
    Summary(s => s.Summary = "Begin passkey step-up ceremony");
  }
}

public class CompletePasskeyStepUp(IMediator mediator)
  : BaseEndpoint<CompletePasskeyStepUpCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/passkeys/step-up/complete");
    Summary(s => s.Summary = "Complete passkey step-up ceremony");
  }
}

public class ResetUserPasskeys(IMediator mediator)
  : BaseEndpoint<ResetUserPasskeysCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/reset-passkeys");
    Summary(s => s.Summary = "Remove all passkeys for a user and revoke sessions");
  }
}

public sealed class ResetUserPasskeysCommandValidator : Validator<ResetUserPasskeysCommand>
{
  public ResetUserPasskeysCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
