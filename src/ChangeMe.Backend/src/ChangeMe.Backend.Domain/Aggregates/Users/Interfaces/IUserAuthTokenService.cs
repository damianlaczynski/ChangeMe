using ChangeMe.Backend.Domain.Aggregates.Users.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IUserAuthTokenService
{
  Task<Result<string>> IssueTokenAsync(
    Guid userId,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default);

  Task<Result<Guid>> ValidateTokenAsync(
    string plainToken,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default);

  Task MarkTokenUsedAsync(
    string plainToken,
    CancellationToken cancellationToken = default);
}
