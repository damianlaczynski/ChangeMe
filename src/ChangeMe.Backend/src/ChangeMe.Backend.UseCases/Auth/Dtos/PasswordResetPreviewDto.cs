namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record PasswordResetPreviewDto
{
  public bool IsValid { get; init; }
}
