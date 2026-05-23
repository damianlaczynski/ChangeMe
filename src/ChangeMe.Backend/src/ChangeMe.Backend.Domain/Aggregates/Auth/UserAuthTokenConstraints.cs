namespace ChangeMe.Backend.Domain.Aggregates.Auth;

public static class UserAuthTokenConstraints
{
  public const int TOKEN_BYTES = 32;
  public const int TOKEN_HASH_MAX_LENGTH = 128;
}
