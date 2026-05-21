using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Users.Dtos;

using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record UpdateUserCommand(
  Guid Id,
  string FirstName,
  string LastName,
  string Email,
  IReadOnlyList<Guid>? RoleIds,
  UserStatus? Status) : ICommand<UserDetailsDto>;

public class UpdateUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateUserCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var normalizedEmail = User.NormalizeEmail(command.Email);
    var emailTaken = await context.Users.AnyAsync(
      x => x.NormalizedEmail == normalizedEmail && x.Id != user.Id,
      cancellationToken);

    if (emailTaken)
      return Result<UserDetailsDto>.Conflict(UsersUtils.DuplicateEmailMessage);

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

      var roleResult = await UsersUtils.ReplaceUserRolesAsync(
        context,
        user.Id,
        command.RoleIds,
        currentUserId,
        cancellationToken);

      if (!roleResult.IsSuccess)
        return roleResult.Map();
    }

    if (command.Status.HasValue)
    {
      if (!userAccessor.HasPermission(PermissionCodes.UsersDeactivate))
        return Result<UserDetailsDto>.Forbidden(UsersUtils.PermissionDeniedMessage);

      if (editingSelf && command.Status.Value == UserStatus.Inactive)
        return Result<UserDetailsDto>.Error(UsersUtils.CannotDeactivateOwnAccountMessage);

      if (command.Status.Value == UserStatus.Inactive && user.IsActive)
      {
        user.Deactivate();
        await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
      }
      else if (command.Status.Value == UserStatus.Active && !user.IsActive)
      {
        user.Activate();
      }
    }

    await context.SaveChangesAsync(cancellationToken);

    var updatedUserResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    if (!updatedUserResult.IsSuccess)
      return updatedUserResult.Map();

    return updatedUserResult;
  }
}
