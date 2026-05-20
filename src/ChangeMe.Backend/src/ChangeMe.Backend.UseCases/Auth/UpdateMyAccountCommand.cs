using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record UpdateMyAccountCommand(
  string FirstName,
  string LastName) : ICommand<MyAccountDto>;

public class UpdateMyAccountHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateMyAccountCommand, MyAccountDto>
{
  public async Task<Result<MyAccountDto>> Handle(UpdateMyAccountCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    var updateResult = user.UpdateProfile(command.FirstName, command.LastName);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    var effectivePermissions = await UsersSupport.GetEffectivePermissionsForUserAsync(
      context,
      userId,
      cancellationToken);

    return Result.Success(new MyAccountDto(
      user.Id,
      user.FirstName,
      user.LastName,
      user.Email,
      user.Status.ToString(),
      user.CreatedAt,
      effectivePermissions));
  }
}
