namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class SessionOptions
{
  public int PersistentSessionLifetimeDays { get; set; } = 14;
  public int BrowserSessionLifetimeDays { get; set; } = 1;
}
