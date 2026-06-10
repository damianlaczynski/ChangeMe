namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record EmailVerificationAckDto
{
  public string Message { get; init; } =
    "If an unverified account exists for this email, a verification link has been sent.";
}
