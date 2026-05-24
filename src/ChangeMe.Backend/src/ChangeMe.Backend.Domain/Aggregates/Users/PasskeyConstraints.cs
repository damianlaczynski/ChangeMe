namespace ChangeMe.Backend.Domain.Aggregates.Users;

public static class PasskeyConstraints
{
  public const int NAME_MAX_LENGTH = 100;
  public const int CREDENTIAL_ID_MAX_LENGTH = 1024;
  public const int PUBLIC_KEY_MAX_LENGTH = 4096;
  public const int AUTHENTICATOR_TYPE_MAX_LENGTH = 32;
  public const int CEREMONY_OPTIONS_JSON_MAX_LENGTH = 64_000;
}
