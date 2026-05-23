using ChangeMe.Backend.Domain.Aggregates.Roles;
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
  bool? Deactivated) : ICommand<UserDetailsDto>;

public class UpdateUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateUserCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await context.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
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
        user.Deactivate();
        await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
      }
      else if (!command.Deactivated.Value && !user.IsActive)
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
