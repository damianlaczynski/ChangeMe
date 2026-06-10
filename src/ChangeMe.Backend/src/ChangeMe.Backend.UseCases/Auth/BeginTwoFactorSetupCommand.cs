using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record BeginTwoFactorSetupCommand(string? CurrentPassword)
  : ICommand<BeginTwoFactorSetupResponseDto>;

public class BeginTwoFactorSetupHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<BeginTwoFactorSetupCommand, BeginTwoFactorSetupResponseDto>
{
  public async ValueTask<Result<BeginTwoFactorSetupResponseDto>> Handle(
    BeginTwoFactorSetupCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<BeginTwoFactorSetupResponseDto>.Unauthorized();

    if (!twoFactorPolicyEvaluator.IsTwoFactorEnabledForDeployment())
      return Result<BeginTwoFactorSetupResponseDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<BeginTwoFactorSetupResponseDto>.Unauthorized();

    if (user.TwoFactorEnabled)
      return Result<BeginTwoFactorSetupResponseDto>.Error("Two-factor authentication is already enabled.");

    if (user.HasPasswordSet)
    {
      if (string.IsNullOrWhiteSpace(command.CurrentPassword))
      {
        return Result<BeginTwoFactorSetupResponseDto>.Invalid(new ValidationError(
          nameof(command.CurrentPassword),
          "Current password is required."));
      }

      if (!passwordHasher.VerifyPassword(user.PasswordHash, command.CurrentPassword))
      {
        return Result<BeginTwoFactorSetupResponseDto>.Invalid(new ValidationError(
          nameof(command.CurrentPassword),
          "Current password is incorrect."));
      }
    }

    var utcNow = DateTime.UtcNow;
    var sharedSecret = totpService.GenerateSecret();
    var protectedSecret = secretProtector.Protect(sharedSecret);
    var pendingResult = TwoFactorEnrollmentPending.Create(
      userId,
      protectedSecret,
      utcNow.AddMinutes(15));
    if (!pendingResult.IsSuccess)
      return Result<BeginTwoFactorSetupResponseDto>.Invalid(pendingResult.ValidationErrors);

    await context.TwoFactorEnrollmentPending
      .Where(x => x.UserId == userId)
      .ExecuteDeleteAsync(cancellationToken);

    await context.TwoFactorEnrollmentPending.AddAsync(pendingResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var twoFactorOptions = authOptions.Value.TwoFactor;
    return Result.Success(new BeginTwoFactorSetupResponseDto(
      sharedSecret,
      totpService.BuildProvisioningUri(sharedSecret, user.Email),
      twoFactorOptions.TotpIssuerName));
  }
}
