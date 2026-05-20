using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed record UpdateRoleCommand(
  Guid Id,
  string Name,
  string? Description,
  IReadOnlyList<string> PermissionCodes) : ICommand<RoleDetailsDto>;

public class UpdateRoleHandler(
  ApplicationDbContext context) : ICommandHandler<UpdateRoleCommand, RoleDetailsDto>
{
  public async Task<Result<RoleDetailsDto>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
  {
    var role = await context.Roles
      .Include(x => x.Permissions)
      .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

    if (role is null)
      return Result<RoleDetailsDto>.NotFound();

    if (role.IsSystem)
      return Result<RoleDetailsDto>.Error(RolesSupport.SystemRoleCannotBeModifiedMessage);

    if (await RolesSupport.IsNameTakenAsync(context, command.Name, role.Id, cancellationToken))
      return Result<RoleDetailsDto>.Conflict(RolesSupport.DuplicateNameMessage);

    var permissionValidation = RolesSupport.ValidatePermissionCodes(command.PermissionCodes);
    if (!permissionValidation.IsSuccess)
      return permissionValidation.Map();

    var updateResult = role.UpdateProfile(command.Name, command.Description);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    role.SetPermissions(command.PermissionCodes);
    await context.SaveChangesAsync(cancellationToken);

    return await new GetRoleByIdHandler(context)
      .Handle(new GetRoleByIdQuery { Id = role.Id }, cancellationToken);
  }
}
