using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RegisterUserCommand(
  string FirstName,
  string LastName,
  string Email,
  string Password) : ICommand<AuthResponseDto>;

public class RegisterUserHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<RegisterUserCommand, AuthResponseDto>
{
  public async Task<Result<AuthResponseDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var userExists = await context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (userExists)
      return Result<AuthResponseDto>.Conflict(AuthSessionSupport.DuplicateEmailMessage);

    var defaultRole = await context.Roles
      .FirstOrDefaultAsync(x => x.Name == RoleConstraints.UserRoleName, cancellationToken);

    if (defaultRole is null)
      return Result<AuthResponseDto>.CriticalError("Default user role is not configured.");

    var passwordHash = passwordHasher.HashPassword(command.Password);
    var createUserResult = User.Create(command.FirstName, command.LastName, command.Email, passwordHash);
    if (!createUserResult.IsSuccess)
      return createUserResult.Map();

    var user = createUserResult.Value;
    user.AssignRole(defaultRole.Id);
    await context.Users.AddAsync(user, cancellationToken);

    var sessionResult = await CreateBrowserSessionAsync(user, cancellationToken);
    if (!sessionResult.IsSuccess)
      return Result<AuthResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var authResponse = await AuthSessionSupport.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      cancellationToken);

    if (!authResponse.IsSuccess)
      return authResponse.Map();

    return Result<AuthResponseDto>.Created(authResponse.Value, $"/users/{user.Id}");
  }

  private async Task<Result<(UserSession Session, string RefreshToken)>> CreateBrowserSessionAsync(
    User user,
    CancellationToken cancellationToken)
  {
    const bool rememberMe = false;
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
