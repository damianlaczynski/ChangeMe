namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record RegisterUserResponseDto
{
  public bool RequiresEmailVerification { get; init; }
  public AuthResponseDto? AuthSession { get; init; }
}
