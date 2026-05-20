using ChangeMe.Backend.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed record UpdateRoleUsersCommand(Guid RoleId, IReadOnlyList<Guid> UserIds) : ICommand<bool>;

public class UpdateRoleUsersHandler(
  ApplicationDbContext context) : ICommandHandler<UpdateRoleUsersCommand, bool>
{
  public async Task<Result<bool>> Handle(UpdateRoleUsersCommand command, CancellationToken cancellationToken)
  {
    var roleExists = await context.Roles.AsNoTracking().AnyAsync(x => x.Id == command.RoleId, cancellationToken);
    if (!roleExists)
      return Result<bool>.NotFound();

    var distinctUserIds = command.UserIds.Distinct().ToList();
    if (distinctUserIds.Count > 0)
    {
      var existingUserCount = await context.Users
        .CountAsync(x => distinctUserIds.Contains(x.Id), cancellationToken);

      if (existingUserCount != distinctUserIds.Count)
        return Result<bool>.NotFound();
    }

    var currentlyAssignedUsers = await context.Users
      .Include(x => x.Roles)
      .Where(u => u.Roles.Any(ur => ur.RoleId == command.RoleId))
      .ToListAsync(cancellationToken);

    var newUserIds = distinctUserIds.ToHashSet();
    var usersToRemove = currentlyAssignedUsers
      .Where(u => !newUserIds.Contains(u.Id))
      .ToList();

    foreach (var user in usersToRemove)
    {
      var removeResult = user.RemoveRole(command.RoleId);
      if (!removeResult.IsSuccess)
      {
        if (removeResult.Status == ResultStatus.Error)
          return Result<bool>.Error(string.Format(RolesSupport.UserWouldHaveNoRolesMessage, user.FullName));

        return removeResult.Map();
      }
    }

    if (distinctUserIds.Count > 0)
    {
      var usersToAssign = await context.Users
        .Include(x => x.Roles)
        .Where(u => distinctUserIds.Contains(u.Id))
        .ToListAsync(cancellationToken);

      foreach (var user in usersToAssign)
      {
        if (!user.HasRole(command.RoleId))
          user.AssignRole(command.RoleId);
      }
    }

    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
