namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record InvitationPreviewDto
{
  public bool IsValid { get; init; }
  public string Email { get; init; } = string.Empty;
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
}
