using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

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
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  UserEmailVerificationService emailVerificationService,
  IOptions<AuthOptions> authOptions,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<RegisterUserCommand, RegisterUserResponseDto>
{
  public async Task<Result<RegisterUserResponseDto>> Handle(
    RegisterUserCommand command,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    if (!auth.Registration.PublicEnabled)
      return Result<RegisterUserResponseDto>.Forbidden(AuthSessionUtils.RegistrationDisabledMessage);

    var normalizedEmail = User.NormalizeEmail(command.Email);
    var existingUser = await context.Users
      .Include(x => x.AccountInvitations)
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

    if (existingUser is not null)
    {
      if (!CanCompleteExistingAccount(existingUser))
        return Result<RegisterUserResponseDto>.Conflict(AuthSessionUtils.DuplicateEmailMessage);

      return await CompleteExistingAccountAsync(existingUser, command, auth, cancellationToken);
    }

    var defaultRole = await context.Roles
      .FirstOrDefaultAsync(x => x.Name == RoleConstraints.UserRoleName, cancellationToken);

    if (defaultRole is null)
      return Result<RegisterUserResponseDto>.CriticalError("Default user role is not configured.");

    var passwordHash = passwordHasher.HashPassword(command.Password);
    var createUserResult = User.CreateWithPassword(
      command.FirstName,
      command.LastName,
      command.Email,
      passwordHash,
      emailVerified: false);
    if (!createUserResult.IsSuccess)
      return createUserResult.Map();

    var user = createUserResult.Value;
    user.AssignRole(defaultRole.Id);
    await context.Users.AddAsync(user, cancellationToken);

    if (auth.EmailVerification.Enabled)
    {
      await context.SaveChangesAsync(cancellationToken);

      var verificationResult = await emailVerificationService.SendVerificationAsync(user, cancellationToken);
      if (!verificationResult.IsSuccess)
        return verificationResult.Map();

      return Result<RegisterUserResponseDto>.Created(
        new RegisterUserResponseDto { RequiresEmailVerification = true },
        $"/users/{user.Id}");
    }

    return await CreateSessionAndReturnAsync(user, cancellationToken);
  }

  private static bool CanCompleteExistingAccount(User user) =>
    UsersStatusUtils.ComputeStatus(user) == UserMembershipStatus.InvitationCanceled;

  private async Task<Result<RegisterUserResponseDto>> CompleteExistingAccountAsync(
    User user,
    RegisterUserCommand command,
    AuthOptions auth,
    CancellationToken cancellationToken)
  {
    var profileResult = user.UpdateProfile(command.FirstName, command.LastName);
    if (!profileResult.IsSuccess)
      return profileResult.Map();

    var passwordHash = passwordHasher.HashPassword(command.Password);
    var passwordResult = user.SetPasswordHash(passwordHash);
    if (!passwordResult.IsSuccess)
      return passwordResult.Map();

    if (auth.EmailVerification.Enabled && !user.EmailVerified)
    {
      await context.SaveChangesAsync(cancellationToken);

      var verificationResult = await emailVerificationService.SendVerificationAsync(user, cancellationToken);
      if (!verificationResult.IsSuccess)
        return verificationResult.Map();

      return Result<RegisterUserResponseDto>.Created(
        new RegisterUserResponseDto { RequiresEmailVerification = true },
        $"/users/{user.Id}");
    }

    return await CreateSessionAndReturnAsync(user, cancellationToken);
  }

  private async Task<Result<RegisterUserResponseDto>> CreateSessionAndReturnAsync(
    User user,
    CancellationToken cancellationToken)
  {
    var sessionResult = await AuthSessionFactory.CreateSessionAsync(
      context,
      sessionLifetime,
      httpContextAccessor,
      user,
      cancellationToken,
      SignInMethods.Registration);
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
      passwordExpirationEvaluator.GetPasswordExpiresAtUtc(user),
      twoFactorSetupRequired: false,
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
}
