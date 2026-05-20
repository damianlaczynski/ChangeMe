using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record PreviewEffectivePermissionsCommand(IReadOnlyList<Guid> RoleIds)
  : ICommand<IReadOnlyList<EffectivePermissionDto>>;

public class PreviewEffectivePermissionsHandler(ApplicationDbContext context)
  : ICommandHandler<PreviewEffectivePermissionsCommand, IReadOnlyList<EffectivePermissionDto>>
{
  public async Task<Result<IReadOnlyList<EffectivePermissionDto>>> Handle(
    PreviewEffectivePermissionsCommand command,
    CancellationToken cancellationToken)
  {
    var permissions = await UsersSupport.GetEffectivePermissionsForRolesAsync(
      context,
      command.RoleIds,
      cancellationToken);

    return Result.Success(permissions);
  }
}
