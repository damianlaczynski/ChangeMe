using ChangeMe.Backend.Domain.Aggregates.Sessions;

namespace ChangeMe.Backend.Infrastructure.Auth;

public interface ISessionLifetimeService
{
  int PersistentSessionLifetimeDays { get; }

  int BrowserSessionLifetimeDays { get; }

  bool IsActive(UserSession session, DateTime utcNow);

  DateTime GetRefreshTokenExpiresAtUtc(bool isPersistent, DateTime signedInAtUtc);
}
