using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Support;

internal sealed class FakeUserAccessor : IUserAccessor
{
  public Guid? UserId { get; set; }
  public Guid? SessionId { get; set; }

  public bool HasPermission(string permissionCode) => true;
}

internal sealed class FakeAuthEmailService : IAuthEmailService
{
  public int PasswordChangedEmailsSent { get; private set; }

  public Task<Result> SendAccountInvitationAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasswordResetRequestedAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasswordResetCompletedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasswordChangedAsync(
    User user,
    CancellationToken cancellationToken = default)
  {
    PasswordChangedEmailsSent++;
    return Task.FromResult(Result.Success());
  }

  public Task<Result> SendVerifyEmailAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());
}

internal static class TestAuthOptions
{
  public static IOptions<AuthOptions> Create(
    bool emailVerificationEnabled = false,
    bool publicRegistrationEnabled = true,
    bool passwordExpirationEnabled = false,
    int maximumPasswordAgeDays = 90) =>
    Options.Create(new AuthOptions
    {
      EmailVerificationEnabled = emailVerificationEnabled,
      PublicRegistrationEnabled = publicRegistrationEnabled,
      PasswordExpirationEnabled = passwordExpirationEnabled,
      MaximumPasswordAgeDays = maximumPasswordAgeDays,
      Jwt = new JwtOptions
      {
        Issuer = "ChangeMe.Tests",
        Audience = "ChangeMe.Tests",
        SigningKey = "Integration-Tests-Signing-Key-Needs-32-Chars",
        ExpirationMinutes = 60
      },
      Session = new AuthSessionOptions
      {
        BrowserSessionLifetimeDays = 1,
        PersistentSessionLifetimeDays = 14
      }
    });
}
