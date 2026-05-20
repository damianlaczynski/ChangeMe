using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetManageRoleUsersFormQuery : IQuery<ManageRoleUsersFormDto>
{
  public Guid Id { get; set; }
}

public class GetManageRoleUsersFormHandler(
  ApplicationDbContext context) : IQueryHandler<GetManageRoleUsersFormQuery, ManageRoleUsersFormDto>
{
  public async Task<Result<ManageRoleUsersFormDto>> Handle(
    GetManageRoleUsersFormQuery query,
    CancellationToken cancellationToken)
  {
    var role = await context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
    if (role is null)
      return Result<ManageRoleUsersFormDto>.NotFound();

    var assignedUserIds = await context.Users
      .AsNoTracking()
      .Where(u => u.Roles.Any(ur => ur.RoleId == role.Id))
      .Select(u => u.Id)
      .ToListAsync(cancellationToken);

    var availableUsers = await context.Users
      .AsNoTracking()
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .Select(u => new UserAssignmentOptionDto
      {
        Id = u.Id,
        FullName = u.FirstName + " " + u.LastName,
        Email = u.Email,
        Status = u.Status
      })
      .ToListAsync(cancellationToken);

    return Result.Success(new ManageRoleUsersFormDto
    {
      RoleId = role.Id,
      RoleName = role.Name,
      IsSystem = role.IsSystem,
      AssignedUserIds = assignedUserIds,
      AvailableUsers = availableUsers
    });
  }
}
