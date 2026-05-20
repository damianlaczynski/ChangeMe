using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record GetUserFormQuery(Guid Id) : IQuery<UserFormDto>;

public class GetUserFormHandler(
  ApplicationDbContext context) : IQueryHandler<GetUserFormQuery, UserFormDto>
{
  public async Task<Result<UserFormDto>> Handle(GetUserFormQuery query, CancellationToken cancellationToken)
  {
    var user = await context.Users
      .AsNoTracking()
      .Include(x => x.Roles)
      .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
    if (user is null)
      return Result<UserFormDto>.NotFound();

    var roleIds = user.Roles.Select(x => x.RoleId).ToList();

    return Result.Success(new UserFormDto
    {
      Id = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email,
      Status = user.Status,
      RoleIds = roleIds
    });
  }
}
