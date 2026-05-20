using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.UseCases.Issues;

public record GetAssignableUsersQuery(bool doNothing = false) : IQuery<List<IssueAssignableUserDto>>;

public class GetAssignableUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetAssignableUsersQuery, List<IssueAssignableUserDto>>
{
  public async Task<Result<List<IssueAssignableUserDto>>> Handle(
    GetAssignableUsersQuery query,
    CancellationToken cancellationToken)
  {
    var users = await context.Users
      .AsNoTracking()
      .Where(u => u.Status == UserStatus.Active)
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .Select(u => new IssueAssignableUserDto
      {
        Id = u.Id,
        FullName = u.FirstName + " " + u.LastName,
      })
      .ToListAsync(cancellationToken);

    return Result.Success(users);
  }
}
