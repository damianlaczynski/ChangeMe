using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
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
  public string? LastPlainToken { get; private set; }

  public Task<Result> SendAccountInvitationAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default)
  {
    LastPlainToken = plainToken;
    return Task.FromResult(Result.Success());
  }

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

  public Task<Result> SendTwoFactorEnabledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendTwoFactorDisabledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendTwoFactorResetByAdminAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendRecoveryCodeUsedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendExternalAccountLinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendExternalAccountUnlinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasskeyAddedAsync(
    User user,
    string passkeyName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasskeyRemovedAsync(
    User user,
    string passkeyName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasskeysResetByAdminAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());
}

internal sealed class FailingAuthEmailService : IAuthEmailService
{
  public const string DefaultErrorMessage = "The email could not be sent. Please try again.";

  public Task<Result> SendAccountInvitationAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Error(DefaultErrorMessage));

  public Task<Result> SendPasswordResetRequestedAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Error(DefaultErrorMessage));

  public Task<Result> SendPasswordResetCompletedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasswordChangedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendVerifyEmailAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Error(DefaultErrorMessage));

  public Task<Result> SendTwoFactorEnabledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendTwoFactorDisabledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendTwoFactorResetByAdminAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendRecoveryCodeUsedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendExternalAccountLinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendExternalAccountUnlinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Success());

  public Task<Result> SendPasskeyAddedAsync(
    User user,
    string passkeyName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Error(DefaultErrorMessage));

  public Task<Result> SendPasskeyRemovedAsync(
    User user,
    string passkeyName,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Error(DefaultErrorMessage));

  public Task<Result> SendPasskeysResetByAdminAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    Task.FromResult(Result.Error(DefaultErrorMessage));
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
      EmailVerification = new EmailVerificationOptions { Enabled = emailVerificationEnabled },
      Registration = new RegistrationOptions { PublicEnabled = publicRegistrationEnabled },
      PasswordExpiration = new PasswordExpirationOptions
      {
        Enabled = passwordExpirationEnabled,
        MaximumPasswordAgeDays = maximumPasswordAgeDays
      },
      Jwt = new JwtOptions
      {
        Issuer = "ChangeMe.Tests",
        Audience = "ChangeMe.Tests",
        SigningKey = "Integration-Tests-Signing-Key-Needs-32-Chars",
        ExpirationMinutes = 60,
        SessionLifetimeDays = 14
      }
    });
}
