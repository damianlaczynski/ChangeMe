using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users;

using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetMyAccountQuery(bool doNothing = false) : IQuery<MyAccountDto>;

public class GetMyAccountHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetMyAccountQuery, MyAccountDto>
{
  public async Task<Result<MyAccountDto>> Handle(GetMyAccountQuery query, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    var effectivePermissions = await UsersUtils.GetEffectivePermissionsForUserAsync(
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
