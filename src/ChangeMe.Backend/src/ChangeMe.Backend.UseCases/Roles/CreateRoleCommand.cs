using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed record CreateRoleCommand(
  string Name,
  string? Description,
  IReadOnlyList<string> PermissionCodes) : ICommand<RoleDetailsDto>;

public class CreateRoleHandler(
  ApplicationDbContext context) : ICommandHandler<CreateRoleCommand, RoleDetailsDto>
{
  public async Task<Result<RoleDetailsDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
  {
    if (await RolesSupport.IsNameTakenAsync(context, command.Name, null, cancellationToken))
      return Result<RoleDetailsDto>.Conflict(RolesSupport.DuplicateNameMessage);

    var permissionValidation = RolesSupport.ValidatePermissionCodes(command.PermissionCodes);
    if (!permissionValidation.IsSuccess)
      return permissionValidation.Map();

    var createResult = Role.Create(command.Name, command.Description);
    if (!createResult.IsSuccess)
      return createResult.Map();

    var role = createResult.Value;
    role.SetPermissions(command.PermissionCodes);
    await context.Roles.AddAsync(role, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return await new GetRoleByIdHandler(context)
      .Handle(new GetRoleByIdQuery { Id = role.Id }, cancellationToken);
  }
}
