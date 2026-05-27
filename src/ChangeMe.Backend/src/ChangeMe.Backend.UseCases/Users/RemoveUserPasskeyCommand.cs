using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record RemoveUserPasskeyCommand(Guid Id, Guid PasskeyId) : ICommand<UserDetailsDto>;

public class RemoveUserPasskeyHandler(
  ApplicationDbContext context,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
  IAuthEmailService authEmailService,
  IOptions<AuthOptions> authOptions,
  IMediator mediator) : ICommandHandler<RemoveUserPasskeyCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    RemoveUserPasskeyCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicyEvaluator.IsPasskeysEnabledForDeployment())
      return Result<UserDetailsDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var credential = await context.PasskeyCredentials
      .FirstOrDefaultAsync(
        x => x.Id == command.PasskeyId && x.UserId == command.Id,
        cancellationToken);
    if (credential is null)
      return Result<UserDetailsDto>.NotFound();

    var count = await context.PasskeyCredentials.CountAsync(
      x => x.UserId == command.Id,
      cancellationToken);
    var auth = authOptions.Value;

    if (count == 1 && !user.HasPasswordSet && user.ExternalLogins.Count == 0)
      return Result<UserDetailsDto>.Error(PasskeyAuthUtils.RemoveOnlySignInMethodMessage);

    if (count == 1 && auth.Passkeys.PasskeysAuthenticationRequired)
      return Result<UserDetailsDto>.Error(PasskeyAuthUtils.RemoveRequiredPasskeyMessage);

    var name = credential.Name;
    context.PasskeyCredentials.Remove(credential);
    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendPasskeyRemovedAsync(user, name, cancellationToken);

    return await mediator.Send(new GetUserByIdQuery(command.Id), cancellationToken);
  }
}
