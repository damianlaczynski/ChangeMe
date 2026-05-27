namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record LoginResponseDto
{
  public AuthResponseDto? AuthSession { get; init; }
  public PendingSignInChallengeDto? TwoFactorChallenge { get; init; }
}

public sealed record PendingSignInChallengeDto(Guid ChallengeId);
