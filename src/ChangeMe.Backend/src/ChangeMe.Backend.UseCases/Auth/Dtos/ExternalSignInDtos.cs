namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record BeginExternalSignInResponseDto(string AuthorizationUrl);

public sealed record ExternalSignInResponseDto
{
  public AuthResponseDto? AuthSession { get; init; }
  public PendingSignInChallengeDto? TwoFactorChallenge { get; init; }
  public ExternalAccountLinkRequiredDto? LinkAccountRequired { get; init; }
  public bool AccountLinkCompleted { get; init; }
  public bool ExternalStepUpCompleted { get; init; }
}

public sealed record MyAccountExternalLoginDto(
  string ProviderKey,
  string DisplayName,
  DateTime LinkedAtUtc);

public sealed record ExternalAccountLinkRequiredDto(
  string State,
  string Email,
  string ProviderKey,
  string ProviderDisplayName);
