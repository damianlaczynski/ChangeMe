using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record UpdateUserCommand(
  Guid Id,
  string FirstName,
  string LastName,
  string Email,
  IReadOnlyList<Guid>? RoleIds,
  bool? Deactivated) : ICommand<UserDetailsDto>;

public class UpdateUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IAuthEmailService authEmailService,
  IUserAuthTokenService tokenService,
  IOptions<AuthOptions> authOptions) : ICommandHandler<UpdateUserCommand, UserDetailsDto>
{
  public async ValueTask<Result<UserDetailsDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var normalizedEmail = User.NormalizeEmail(command.Email);
    var emailChanged = !user.NormalizedEmail.Equals(normalizedEmail, StringComparison.Ordinal);
    var previousEmail = user.Email;

    if (emailChanged
        && await UsersUtils.IsProfileEmailTakenAsync(context, normalizedEmail, user.Id, cancellationToken))
      return Result<UserDetailsDto>.Conflict(UsersUtils.DuplicateEmailMessage);

    if (emailChanged)
    {
      user.CancelPendingEmailChange();
      await tokenService.InvalidateUnusedTokensAsync(
        user.Id,
        UserAuthTokenType.EmailChangeConfirmation,
        cancellationToken);
    }

    var updateProfileResult = user.UpdateAdminProfile(command.FirstName, command.LastName, command.Email);
    if (!updateProfileResult.IsSuccess)
      return updateProfileResult.Map();

    var currentUserId = userAccessor.UserId!.Value;
    var editingSelf = currentUserId == user.Id;

    if (command.RoleIds is not null)
    {
      if (!userAccessor.HasPermission(PermissionCodes.RolesManage))
        return Result<UserDetailsDto>.Forbidden(UsersUtils.PermissionDeniedMessage);

      if (editingSelf)
        return Result<UserDetailsDto>.Error(UsersUtils.CannotChangeOwnRolesMessage);

      var distinctRoleIds = command.RoleIds.Distinct().ToList();
      var existingRoleCount = await context.Roles
        .CountAsync(x => distinctRoleIds.Contains(x.Id), cancellationToken);

      if (existingRoleCount != distinctRoleIds.Count)
        return Result.NotFound();

      if (currentUserId == user.Id)
      {
        var administratorRoleId = await context.Roles
          .AsNoTracking()
          .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
          .Select(x => x.Id)
          .FirstOrDefaultAsync(cancellationToken);

        if (administratorRoleId != Guid.Empty
            && user.HasRole(administratorRoleId)
            && !distinctRoleIds.Contains(administratorRoleId))
          return Result.Error(UsersUtils.CannotRemoveOwnAdministratorMessage);
      }

      var roleResult = user.ReplaceRoles(distinctRoleIds);
      if (!roleResult.IsSuccess)
        return roleResult.Map();
    }

    if (command.Deactivated.HasValue)
    {
      if (!userAccessor.HasPermission(PermissionCodes.UsersDeactivate))
        return Result<UserDetailsDto>.Forbidden(UsersUtils.PermissionDeniedMessage);

      if (editingSelf && command.Deactivated.Value)
        return Result<UserDetailsDto>.Error(UsersUtils.CannotDeactivateOwnAccountMessage);

      if (command.Deactivated.Value && user.IsActive)
      {
        var validationResult = await UsersUtils.ValidateCanDeactivateUserAsync(context, user.Id, cancellationToken);
        if (!validationResult.IsSuccess)
          return validationResult.Map();

        user.Deactivate();
        await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
      }
      else if (!command.Deactivated.Value && !user.IsActive)
      {
        user.Activate();
      }
    }
    if (emailChanged)
    {
      if (authOptions.Value.EmailVerification.Enabled)
        user.MarkEmailVerified();

      await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);

    if (emailChanged)
    {
      var emailResult = await authEmailService.SendEmailChangedByAdminAsync(
        previousEmail,
        user.Email,
        cancellationToken);
      if (!emailResult.IsSuccess)
        return emailResult.Map();
    }

    var updatedUserResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    if (!updatedUserResult.IsSuccess)
      return updatedUserResult.Map();

    return updatedUserResult;
  }
}
