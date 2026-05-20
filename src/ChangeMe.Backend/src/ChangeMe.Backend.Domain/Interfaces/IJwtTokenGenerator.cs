using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Domain.Interfaces;

public interface IJwtTokenGenerator
{
  AccessTokenResult GenerateToken(User user, Guid sessionId, IReadOnlyList<string> permissions);
}
