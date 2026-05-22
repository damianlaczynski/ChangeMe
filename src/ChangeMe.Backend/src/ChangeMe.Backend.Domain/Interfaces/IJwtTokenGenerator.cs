using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Domain.Interfaces;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
  AccessTokenResult GenerateToken(User user, Guid sessionId, IReadOnlyList<string> permissions);
}
