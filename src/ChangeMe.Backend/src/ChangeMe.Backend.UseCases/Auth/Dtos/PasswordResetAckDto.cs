namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record PasswordResetAckDto
{
  public string Message { get; init; } =
    "If an account exists for this email, a reset link has been sent.";
}
