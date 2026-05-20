using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;

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
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<LoginUserCommand, AuthResponseDto>
{
  public async Task<Result<AuthResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var user = await context.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (user is null)
      return Result<AuthResponseDto>.Unauthorized(AuthSessionSupport.InvalidCredentialsMessage);

    if (!user.IsActive)
      return Result<AuthResponseDto>.Unauthorized(AuthSessionSupport.DeactivatedAccountMessage);

    if (!passwordHasher.VerifyPassword(user.PasswordHash, command.Password))
      return Result<AuthResponseDto>.Unauthorized(AuthSessionSupport.InvalidCredentialsMessage);

    var sessionResult = await CreateSessionAsync(user, command.RememberMe, cancellationToken);
    if (!sessionResult.IsSuccess)
      return Result<AuthResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    return await AuthSessionSupport.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
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
    var ipAddress = AuthSessionSupport.GetClientIpAddress(httpContext);

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
