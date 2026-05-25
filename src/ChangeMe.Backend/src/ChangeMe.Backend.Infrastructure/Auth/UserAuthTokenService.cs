using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
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
    DateTime? issuedAtUtc = null,
    CancellationToken cancellationToken = default)
  {
    var utcNow = issuedAtUtc ?? timeProvider.GetUtcNow().UtcDateTime;

    await InvalidateUnusedTokensAsync(userId, type, utcNow, cancellationToken);

    var plainToken = SecureTokenGenerator.CreateToken();
    var tokenHash = SecureTokenGenerator.HashToken(plainToken);
    var expiresAtUtc = GetExpiresAtUtc(type, utcNow);

    var createResult = UserAuthToken.Create(userId, type, tokenHash, expiresAtUtc);
    if (!createResult.IsSuccess)
      return createResult.Map();

    await context.UserAuthTokens.AddAsync(createResult.Value, cancellationToken);

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
  }

  public Task InvalidateUnusedTokensAsync(
    Guid userId,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default) =>
    InvalidateUnusedTokensAsync(
      userId,
      type,
      timeProvider.GetUtcNow().UtcDateTime,
      cancellationToken);

  public async Task<DateTime?> GetActiveUnusedTokenExpiresAtUtcAsync(
    Guid userId,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default)
  {
    var token = await context.UserAuthTokens
      .AsNoTracking()
      .Where(x => x.UserId == userId && x.Type == type && x.UsedAtUtc == null)
      .OrderByDescending(x => x.ExpiresAtUtc)
      .FirstOrDefaultAsync(cancellationToken);

    return token?.ExpiresAtUtc;
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
  }

  private DateTime GetExpiresAtUtc(UserAuthTokenType type, DateTime utcNow) =>
    type switch
    {
      UserAuthTokenType.Invitation => utcNow.AddHours(authOptions.Value.Invitations.InvitationLinkLifetimeHours),
      UserAuthTokenType.PasswordReset => utcNow.AddHours(authOptions.Value.PasswordReset.LinkLifetimeHours),
      UserAuthTokenType.EmailVerification => utcNow.AddHours(authOptions.Value.EmailVerification.LinkLifetimeHours),
      _ => utcNow.AddHours(24)
    };
}
