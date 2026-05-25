using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record UnlinkUserExternalLoginCommand(
  Guid UserId,
  string ProviderKey) : ICommand<UserDetailsDto>;

public class UnlinkUserExternalLoginHandler(
  ApplicationDbContext context,
  IAuthEmailService authEmailService,
  IMediator mediator,
  IOptions<AuthOptions> authOptions) : ICommandHandler<UnlinkUserExternalLoginCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    UnlinkUserExternalLoginCommand command,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    if (!auth.External.Enabled)
      return Result<UserDetailsDto>.Forbidden(ExternalAuthUtils.ExternalProvidersDisabledMessage);

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.Roles)
      .ThenInclude(x => x.Role)
      .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var canUnlinkResult = ExternalAuthUtils.ValidateCanUnlinkExternalLogin(user, command.ProviderKey);
    if (!canUnlinkResult.IsSuccess)
      return canUnlinkResult.Map();

    var displayName = ExternalAuthUtils.ResolveProviderDisplayName(auth, command.ProviderKey);
    var removeResult = user.RemoveExternalLogin(command.ProviderKey);
    if (!removeResult.IsSuccess)
      return removeResult.Map();

    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendExternalAccountUnlinkedAsync(user, displayName, cancellationToken);

    return await mediator.Send(new GetUserByIdQuery(command.UserId), cancellationToken);
  }
}
