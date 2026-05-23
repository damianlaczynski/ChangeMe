using ChangeMe.Backend.Domain.Aggregates.Users.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class UserAuthToken : Entity
{
  private UserAuthToken() { }

  public Guid UserId { get; private set; }
  public UserAuthTokenType Type { get; private set; }
  public string TokenHash { get; private set; } = string.Empty;
  public DateTime ExpiresAtUtc { get; private set; }
  public DateTime? UsedAtUtc { get; private set; }

  public bool IsUsed => UsedAtUtc is not null;

  public bool IsValid(DateTime utcNow) => !IsUsed && utcNow < ExpiresAtUtc;

  public static Result<UserAuthToken> Create(
    Guid userId,
    UserAuthTokenType type,
    string tokenHash,
    DateTime expiresAtUtc)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(tokenHash))
      validationErrors.Add(new ValidationError(nameof(TokenHash), "cannot be null or empty"));
    else if (tokenHash.Length > UserAuthTokenConstraints.TOKEN_HASH_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(TokenHash), $"cannot be longer than {UserAuthTokenConstraints.TOKEN_HASH_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new UserAuthToken
    {
      UserId = userId,
      Type = type,
      TokenHash = tokenHash.Trim(),
      ExpiresAtUtc = expiresAtUtc,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty
    });
  }

  public void MarkUsed(DateTime utcNow)
  {
    UsedAtUtc = utcNow;
  }
}

public static class UserAuthTokenConstraints
{
  public const int TOKEN_BYTES = 32;
  public const int TOKEN_HASH_MAX_LENGTH = 128;
}
