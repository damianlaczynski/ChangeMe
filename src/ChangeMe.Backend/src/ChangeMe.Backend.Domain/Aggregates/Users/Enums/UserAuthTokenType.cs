namespace ChangeMe.Backend.Domain.Aggregates.Users.Enums;

public enum UserAuthTokenType
{
  Invitation = 0,
  PasswordReset = 1,
  EmailVerification = 2,
  EmailChangeConfirmation = 3
}
