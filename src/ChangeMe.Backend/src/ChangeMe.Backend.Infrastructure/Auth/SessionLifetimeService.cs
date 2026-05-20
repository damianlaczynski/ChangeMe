using ChangeMe.Backend.Domain.Aggregates.Sessions;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class SessionLifetimeService(IOptions<SessionOptions> options) : ISessionLifetimeService
{
  private readonly SessionOptions sessionOptions = options.Value;

  public int PersistentSessionLifetimeDays =>
    sessionOptions.PersistentSessionLifetimeDays > 0
      ? sessionOptions.PersistentSessionLifetimeDays
      : 14;

  public int BrowserSessionLifetimeDays =>
    sessionOptions.BrowserSessionLifetimeDays > 0
      ? sessionOptions.BrowserSessionLifetimeDays
      : 1;

  public bool IsActive(UserSession session, DateTime utcNow) =>
    session.IsActive(utcNow, PersistentSessionLifetimeDays);

  public DateTime GetRefreshTokenExpiresAtUtc(bool isPersistent, DateTime signedInAtUtc) =>
    isPersistent
      ? signedInAtUtc.AddDays(PersistentSessionLifetimeDays)
      : signedInAtUtc.AddDays(BrowserSessionLifetimeDays);
}
