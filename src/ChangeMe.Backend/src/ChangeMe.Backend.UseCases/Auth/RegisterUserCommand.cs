using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RegisterUserCommand(
  string FirstName,
  string LastName,
  string Email,
  string Password) : ICommand<RegisterUserResponseDto>;

public class RegisterUserHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  UserEmailVerificationService emailVerificationService,
  IOptions<AuthOptions> authOptions,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<RegisterUserCommand, RegisterUserResponseDto>
{
  public async Task<Result<RegisterUserResponseDto>> Handle(
    RegisterUserCommand command,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    if (!auth.PublicRegistrationEnabled)
      return Result<RegisterUserResponseDto>.Forbidden(AuthSessionUtils.RegistrationDisabledMessage);

    var normalizedEmail = User.NormalizeEmail(command.Email);
    var userExists = await context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (userExists)
      return Result<RegisterUserResponseDto>.Conflict(AuthSessionUtils.DuplicateEmailMessage);

    var defaultRole = await context.Roles
      .FirstOrDefaultAsync(x => x.Name == RoleConstraints.UserRoleName, cancellationToken);

    if (defaultRole is null)
      return Result<RegisterUserResponseDto>.CriticalError("Default user role is not configured.");

    var emailVerified = !auth.EmailVerificationEnabled;
    var passwordHash = passwordHasher.HashPassword(command.Password);
    var createUserResult = User.CreateWithPassword(
      command.FirstName,
      command.LastName,
      command.Email,
      passwordHash,
      emailVerified);
    if (!createUserResult.IsSuccess)
      return createUserResult.Map();

    var user = createUserResult.Value;
    user.AssignRole(defaultRole.Id);
    await context.Users.AddAsync(user, cancellationToken);

    if (auth.EmailVerificationEnabled)
    {
      await context.SaveChangesAsync(cancellationToken);

      var verificationResult = await emailVerificationService.SendVerificationAsync(user, cancellationToken);
      if (!verificationResult.IsSuccess)
        return verificationResult.Map();

      return Result<RegisterUserResponseDto>.Created(
        new RegisterUserResponseDto { RequiresEmailVerification = true },
        $"/users/{user.Id}");
    }

    var sessionResult = await CreateBrowserSessionAsync(user, cancellationToken);
    if (!sessionResult.IsSuccess)
      return Result<RegisterUserResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var authResponse = await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      passwordChangeRequired: false,
      cancellationToken);

    if (!authResponse.IsSuccess)
      return authResponse.Map();

    return Result<RegisterUserResponseDto>.Created(
      new RegisterUserResponseDto
      {
        RequiresEmailVerification = false,
        AuthSession = authResponse.Value
      },
      $"/users/{user.Id}");
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
