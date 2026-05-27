namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class PasskeyCredential
{
  private PasskeyCredential() { }

  public Guid Id { get; private set; } = Guid.CreateVersion7();
  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public string Name { get; private set; } = string.Empty;
  public byte[] CredentialId { get; private set; } = [];
  public byte[] PublicKey { get; private set; } = [];
  public uint SignCount { get; private set; }
  public Guid Aaguid { get; private set; }
  public string AuthenticatorType { get; private set; } = string.Empty;
  public bool BackupEligible { get; private set; }
  public bool BackupState { get; private set; }
  public DateTime CreatedAtUtc { get; private set; }
  public DateTime? LastUsedAtUtc { get; private set; }

  public static Result<PasskeyCredential> Create(
    Guid userId,
    string name,
    byte[] credentialId,
    byte[] publicKey,
    uint signCount,
    Guid aaguid,
    string authenticatorType,
    bool backupEligible,
    bool backupState,
    DateTime createdAtUtc)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    ValidateName(name, validationErrors);

    if (credentialId.Length == 0)
      validationErrors.Add(new ValidationError(nameof(CredentialId), "cannot be empty"));
    else if (credentialId.Length > PasskeyCredentialConstraints.CREDENTIAL_ID_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(CredentialId), "is too long"));

    if (publicKey.Length == 0)
      validationErrors.Add(new ValidationError(nameof(PublicKey), "cannot be empty"));
    else if (publicKey.Length > PasskeyCredentialConstraints.PUBLIC_KEY_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(PublicKey), "is too long"));

    if (string.IsNullOrWhiteSpace(authenticatorType))
      validationErrors.Add(new ValidationError(nameof(AuthenticatorType), "cannot be null or empty"));
    else if (authenticatorType.Length > PasskeyCredentialConstraints.AUTHENTICATOR_TYPE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(AuthenticatorType), "is too long"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new PasskeyCredential
    {
      UserId = userId,
      Name = name.Trim(),
      CredentialId = credentialId,
      PublicKey = publicKey,
      SignCount = signCount,
      Aaguid = aaguid,
      AuthenticatorType = authenticatorType.Trim(),
      BackupEligible = backupEligible,
      BackupState = backupState,
      CreatedAtUtc = createdAtUtc
    });
  }

  public void Rename(string name)
  {
    var validationErrors = new List<ValidationError>();
    ValidateName(name, validationErrors);
    if (validationErrors.Count > 0)
      throw new InvalidOperationException(string.Join("; ", validationErrors.Select(x => x.ErrorMessage)));

    Name = name.Trim();
  }

  public void RecordUse(uint signCount, DateTime utcNow)
  {
    SignCount = signCount;
    LastUsedAtUtc = utcNow;
  }

  private static void ValidateName(string name, ICollection<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(name))
      validationErrors.Add(new ValidationError(nameof(Name), "cannot be null or empty"));
    else if (name.Trim().Length > PasskeyCredentialConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Name), $"cannot be longer than {PasskeyCredentialConstraints.NAME_MAX_LENGTH} characters"));
  }
}

public static class PasskeyCredentialConstraints
{
  public const int NAME_MAX_LENGTH = 100;
  public const int CREDENTIAL_ID_MAX_LENGTH = 1024;
  public const int PUBLIC_KEY_MAX_LENGTH = 4096;
  public const int AUTHENTICATOR_TYPE_MAX_LENGTH = 32;
}
