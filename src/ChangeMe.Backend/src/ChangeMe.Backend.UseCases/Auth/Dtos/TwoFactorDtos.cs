namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record VerifyTwoFactorCommandDto(
  Guid ChallengeId,
  string VerificationCode);

public sealed record BeginTwoFactorSetupResponseDto(
  string SharedSecret,
  string ProvisioningUri,
  string IssuerName);

public sealed record ConfirmTwoFactorSetupCommandDto(
  string VerificationCode,
  string? CurrentPassword);

public sealed record TwoFactorSetupCompletedDto(
  IReadOnlyList<string> RecoveryCodes);
