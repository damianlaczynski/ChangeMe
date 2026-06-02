using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Issues;

public sealed record GetAssignableUsersQuery() : IQuery<List<IssueAssignableUserDto>>;

public class GetAssignableUsersHandler(
  ApplicationDbContext context,
  IOptions<AuthOptions> authOptions)
  : IQueryHandler<GetAssignableUsersQuery, List<IssueAssignableUserDto>>
{
  public async Task<Result<List<IssueAssignableUserDto>>> Handle(
    GetAssignableUsersQuery query,
    CancellationToken cancellationToken)
  {
    var emailVerificationEnabled = authOptions.Value.EmailVerification.Enabled;

    var users = await context.Users
      .AsNoTracking()
      .Where(u =>
        !u.Deactivated
        && u.HasPasswordSet
        && (!emailVerificationEnabled || u.EmailVerified))
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .ToListAsync(cancellationToken);

    var assignableUsers = users
      .Select(u => new IssueAssignableUserDto
      {
        Id = u.Id,
        DisplayLabel = u.DisplayLabel,
      })
      .ToList();

    return Result.Success(assignableUsers);
  }
}
