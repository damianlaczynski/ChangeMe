using ChangeMe.Backend.Domain.Aggregates.Auth;
using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserAuthTokenService(
  ApplicationDbContext context,
  IOptions<AuthOptions> authOptions,
  TimeProvider timeProvider) : IUserAuthTokenService
{
  public async Task<Result<string>> IssueTokenAsync(
    Guid userId,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default)
  {
    var utcNow = timeProvider.GetUtcNow().UtcDateTime;

    await InvalidateUnusedTokensAsync(userId, type, utcNow, cancellationToken);

    var plainToken = SecureTokenGenerator.CreateToken();
    var tokenHash = SecureTokenGenerator.HashToken(plainToken);
    var expiresAtUtc = GetExpiresAtUtc(type, utcNow);

    var createResult = UserAuthToken.Create(userId, type, tokenHash, expiresAtUtc);
    if (!createResult.IsSuccess)
      return createResult.Map();

    await context.UserAuthTokens.AddAsync(createResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(plainToken);
  }

  public async Task<Result<Guid>> ValidateTokenAsync(
    string plainToken,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(plainToken))
      return Result<Guid>.NotFound();

    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
    var tokenHash = SecureTokenGenerator.HashToken(plainToken);

    var token = await context.UserAuthTokens
      .AsNoTracking()
      .FirstOrDefaultAsync(
        x => x.TokenHash == tokenHash && x.Type == type,
        cancellationToken);

    if (token is null || !token.IsValid(utcNow))
      return Result<Guid>.NotFound();

    return Result.Success(token.UserId);
  }

  public async Task MarkTokenUsedAsync(string plainToken, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(plainToken))
      return;

    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
    var tokenHash = SecureTokenGenerator.HashToken(plainToken);

    var token = await context.UserAuthTokens
      .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    if (token is null || token.IsUsed)
      return;

    token.MarkUsed(utcNow);
    await context.SaveChangesAsync(cancellationToken);
  }

  private async Task InvalidateUnusedTokensAsync(
    Guid userId,
    UserAuthTokenType type,
    DateTime utcNow,
    CancellationToken cancellationToken)
  {
    var tokens = await context.UserAuthTokens
      .Where(x => x.UserId == userId && x.Type == type && x.UsedAtUtc == null)
      .ToListAsync(cancellationToken);

    foreach (var token in tokens)
      token.MarkUsed(utcNow);

    if (tokens.Count > 0)
      await context.SaveChangesAsync(cancellationToken);
  }

  private DateTime GetExpiresAtUtc(UserAuthTokenType type, DateTime utcNow) =>
    type switch
    {
      UserAuthTokenType.Invitation => utcNow.AddHours(authOptions.Value.InvitationLinkLifetimeHours),
      UserAuthTokenType.PasswordReset => utcNow.AddHours(authOptions.Value.PasswordResetLinkLifetimeHours),
      UserAuthTokenType.EmailVerification => utcNow.AddHours(authOptions.Value.EmailVerificationLinkLifetimeHours),
      _ => utcNow.AddHours(24)
    };
}
