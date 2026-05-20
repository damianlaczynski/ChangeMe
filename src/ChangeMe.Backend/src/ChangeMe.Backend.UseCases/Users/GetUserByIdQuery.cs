using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDetailsDto>;

public class GetUserByIdHandler(
  ApplicationDbContext context) : IQueryHandler<GetUserByIdQuery, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
  {
    var user = await context.Users
      .AsNoTracking()
      .Include(x => x.Roles)
      .ThenInclude(x => x.Role)
      .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var roles = user.Roles
      .Select(x => x.Role)
      .OrderBy(role => role.Name)
      .Select(role => new UserRoleSummaryDto(role.Id, role.Name, role.IsSystem))
      .ToList();

    var effectivePermissions = await UsersSupport.GetEffectivePermissionsForUserAsync(
      context,
      user.Id,
      cancellationToken);

    var lastSignInAt = await UsersSupport.GetLastSignInAtAsync(context, user.Id, cancellationToken);

    return Result.Success(new UserDetailsDto
    {
      Id = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      FullName = user.FullName,
      Email = user.Email,
      Status = user.Status,
      MemberSince = user.CreatedAt,
      LastSignInAt = lastSignInAt,
      Roles = roles,
      EffectivePermissions = effectivePermissions
    });
  }
}
