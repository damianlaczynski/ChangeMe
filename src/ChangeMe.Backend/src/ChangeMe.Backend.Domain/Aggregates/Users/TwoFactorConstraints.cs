namespace ChangeMe.Backend.Domain.Aggregates.Users;

public static class TwoFactorConstraints
{
  public const int RECOVERY_CODE_LENGTH = 10;
  public const int ENCRYPTED_SECRET_MAX_LENGTH = 512;
  public const int RECOVERY_CODE_HASH_MAX_LENGTH = 128;
  public const int PROVIDER_KEY_MAX_LENGTH = 64;
  public const int PROVIDER_SUBJECT_MAX_LENGTH = 256;
  public const int PROVIDER_DISPLAY_NAME_MAX_LENGTH = 100;
}
