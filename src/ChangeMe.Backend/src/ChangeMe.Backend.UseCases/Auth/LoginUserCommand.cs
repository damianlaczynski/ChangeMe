using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record LoginUserCommand(
  string Email,
  string Password,
  bool RememberMe) : ICommand<AuthResponseDto>;

public class LoginUserHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  IOptions<AuthOptions> authOptions,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<LoginUserCommand, AuthResponseDto>
{
  public async Task<Result<AuthResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var user = await context.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (user is null)
      return Result<AuthResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    if (!user.IsActive)
      return Result<AuthResponseDto>.Unauthorized(AuthSessionUtils.DeactivatedAccountMessage);

    if (!user.HasPasswordSet)
      return Result<AuthResponseDto>.Unauthorized(AuthSessionUtils.InvitePendingAccountMessage);

    if (authOptions.Value.EmailVerificationEnabled && !user.EmailVerified)
      return Result<AuthResponseDto>.Unauthorized(AuthSessionUtils.EmailNotVerifiedMessage);

    if (!passwordHasher.VerifyPassword(user.PasswordHash, command.Password))
      return Result<AuthResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    var sessionResult = await CreateSessionAsync(user, command.RememberMe, cancellationToken);
    if (!sessionResult.IsSuccess)
      return Result<AuthResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var utcNow = DateTime.UtcNow;
    var passwordChangeRequired = passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow);

    return await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      passwordChangeRequired,
      cancellationToken);
  }

  private async Task<Result<(UserSession Session, string RefreshToken)>> CreateSessionAsync(
    User user,
    bool rememberMe,
    CancellationToken cancellationToken)
  {
    var signedInAt = DateTime.UtcNow;
    var refreshToken = RefreshTokenGenerator.CreateToken();
    var refreshTokenHash = RefreshTokenGenerator.HashToken(refreshToken);
    var refreshTokenExpiresAtUtc = sessionLifetime.GetRefreshTokenExpiresAtUtc(rememberMe, signedInAt);
    var httpContext = httpContextAccessor.HttpContext;
    var deviceLabel = ClientInfoParser.ParseDeviceBrowserLabel(httpContext?.Request.Headers.UserAgent);
    var ipAddress = AuthSessionUtils.GetClientIpAddress(httpContext);

    var sessionResult = UserSession.Create(
      user.Id,
      rememberMe,
      deviceLabel,
      ipAddress,
      refreshTokenHash,
      refreshTokenExpiresAtUtc,
      signedInAt);

    if (!sessionResult.IsSuccess)
      return sessionResult.Map();

    await context.UserSessions.AddAsync(sessionResult.Value, cancellationToken);
    return Result.Success((sessionResult.Value, refreshToken));
  }
}