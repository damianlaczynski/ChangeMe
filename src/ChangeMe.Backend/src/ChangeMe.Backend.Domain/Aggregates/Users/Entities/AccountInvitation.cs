namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class AccountInvitation : Entity
{
  private AccountInvitation() { }

  public Guid UserId { get; private set; }
  public DateTime SentAtUtc { get; private set; }
  public DateTime LinkExpiresAtUtc { get; private set; }
  public DateTime? AcceptedAtUtc { get; private set; }
  public DateTime? RevokedAtUtc { get; private set; }

  public bool IsPending => AcceptedAtUtc is null && RevokedAtUtc is null;

  public static Result<AccountInvitation> Create(
    Guid userId,
    DateTime sentAtUtc,
    DateTime linkExpiresAtUtc)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (linkExpiresAtUtc < sentAtUtc)
      validationErrors.Add(new ValidationError(nameof(LinkExpiresAtUtc), "cannot be before sent time"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new AccountInvitation
    {
      UserId = userId,
      SentAtUtc = sentAtUtc,
      LinkExpiresAtUtc = linkExpiresAtUtc
    });
  }

  public void Revoke(DateTime revokedAtUtc) => RevokedAtUtc = revokedAtUtc;

  public void Accept(DateTime acceptedAtUtc) => AcceptedAtUtc = acceptedAtUtc;
}
