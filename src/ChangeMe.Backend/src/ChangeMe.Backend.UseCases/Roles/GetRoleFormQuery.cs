using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRoleFormQuery : IQuery<RoleFormDto>
{
  public Guid Id { get; set; }
}

public class GetRoleFormHandler(
  ApplicationDbContext context) : IQueryHandler<GetRoleFormQuery, RoleFormDto>
{
  public async Task<Result<RoleFormDto>> Handle(GetRoleFormQuery query, CancellationToken cancellationToken)
  {
    var role = await context.Roles
      .AsNoTracking()
      .Include(x => x.Permissions)
      .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
    if (role is null)
      return Result<RoleFormDto>.NotFound();

    var permissionCodes = role.Permissions.Select(x => x.PermissionCode).ToList();

    return Result.Success(new RoleFormDto
    {
      Id = role.Id,
      Name = role.Name,
      Description = role.Description,
      IsSystem = role.IsSystem,
      PermissionCodes = permissionCodes
    });
  }
}
