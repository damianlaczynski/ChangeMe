using System.Net.Mail;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Entities;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Invitations;

public class Invitation : Entity, IAggregateRoot
{
  private readonly List<InvitationRole> roles = [];

  private Invitation() { }

  public string Email { get; private set; } = string.Empty;
  public string NormalizedEmail { get; private set; } = string.Empty;
  public string? FirstName { get; private set; }
  public string? LastName { get; private set; }
  public Guid InvitedByUserId { get; private set; }
  public InvitationStatus Status { get; private set; } = InvitationStatus.PENDING;
  public DateTime ExpiresAt { get; private set; }
  public DateTime? AcceptedAt { get; private set; }
  public string TokenHash { get; private set; } = string.Empty;

  public IReadOnlyCollection<InvitationRole> Roles => roles.AsReadOnly();

  public static Result<Invitation> Create(
    string email,
    string? firstName,
    string? lastName,
    IReadOnlyList<Guid> roleIds,
    Guid invitedByUserId,
    string tokenHash,
    DateTime expiresAt)
  {
    var validationErrors = ValidateCreate(email, firstName, lastName, roleIds, invitedByUserId, tokenHash, expiresAt);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var invitation = new Invitation
    {
      Email = email.Trim(),
      NormalizedEmail = NormalizeEmail(email),
      FirstName = NormalizeOptionalName(firstName),
      LastName = NormalizeOptionalName(lastName),
      InvitedByUserId = invitedByUserId,
      Status = InvitationStatus.PENDING,
      ExpiresAt = expiresAt,
      TokenHash = tokenHash,
      CreatedBy = invitedByUserId,
      UpdatedBy = invitedByUserId,
    };

    foreach (var roleId in roleIds.Distinct())
      invitation.roles.Add(InvitationRole.Create(invitation.Id, roleId));

    return Result.Success(invitation);
  }

  public Result Resend(string tokenHash, DateTime expiresAt)
  {
    if (Status != InvitationStatus.PENDING)
      return Result.Error(InvitationMessages.OnlyPendingCanBeResent);

    if (string.IsNullOrWhiteSpace(tokenHash))
      return Result.Invalid(new ValidationError(nameof(TokenHash), "cannot be empty"));

    TokenHash = tokenHash;
    ExpiresAt = expiresAt;
    return Result.Success();
  }

  public Result Revoke()
  {
    if (Status != InvitationStatus.PENDING)
      return Result.Error(InvitationMessages.OnlyPendingCanBeRevoked);

    Status = InvitationStatus.REVOKED;
    return Result.Success();
  }

  public void ApplyExpiryIfNeeded(DateTime utcNow)
  {
    if (Status == InvitationStatus.PENDING && utcNow > ExpiresAt)
      Status = InvitationStatus.EXPIRED;
  }

  public Result ValidateForOpen(string tokenHash, DateTime utcNow)
  {
    ApplyExpiryIfNeeded(utcNow);

    if (!string.Equals(TokenHash, tokenHash, StringComparison.Ordinal))
      return Result.Error(InvitationMessages.LinkNotValid);

    return Status switch
    {
      InvitationStatus.REVOKED => Result.Error(InvitationMessages.NoLongerValid),
      InvitationStatus.ACCEPTED => Result.Error(InvitationMessages.AlreadyUsed),
      InvitationStatus.EXPIRED => Result.Error(InvitationMessages.Expired),
      InvitationStatus.PENDING when utcNow > ExpiresAt => Result.Error(InvitationMessages.Expired),
      InvitationStatus.PENDING => Result.Success(),
      _ => Result.Error(InvitationMessages.LinkNotValid)
    };
  }

  public Result MarkAccepted(DateTime acceptedAt)
  {
    if (Status != InvitationStatus.PENDING)
      return Result.Error(InvitationMessages.LinkNotValid);

    Status = InvitationStatus.ACCEPTED;
    AcceptedAt = acceptedAt;
    return Result.Success();
  }

  public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

  private static string? NormalizeOptionalName(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : value.Trim();

  private static List<ValidationError> ValidateCreate(
    string email,
    string? firstName,
    string? lastName,
    IReadOnlyList<Guid> roleIds,
    Guid invitedByUserId,
    string tokenHash,
    DateTime expiresAt)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(email))
      validationErrors.Add(new ValidationError(nameof(Email), "cannot be empty"));
    else
    {
      if (email.Trim().Length > InvitationConstraints.EMAIL_MAX_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Email), $"cannot be longer than {InvitationConstraints.EMAIL_MAX_LENGTH} characters"));

      try
      {
        _ = new MailAddress(email.Trim());
      }
      catch (FormatException)
      {
        validationErrors.Add(new ValidationError(nameof(Email), "must be a valid email address"));
      }
    }

    if (firstName is not null && firstName.Trim().Length > InvitationConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(FirstName), $"cannot be longer than {InvitationConstraints.NAME_MAX_LENGTH} characters"));

    if (lastName is not null && lastName.Trim().Length > InvitationConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(LastName), $"cannot be longer than {InvitationConstraints.NAME_MAX_LENGTH} characters"));

    if (roleIds.Count == 0)
      validationErrors.Add(new ValidationError(nameof(Roles), "at least one role is required"));

    if (invitedByUserId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(InvitedByUserId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(tokenHash))
      validationErrors.Add(new ValidationError(nameof(TokenHash), "cannot be empty"));

    if (expiresAt == default)
      validationErrors.Add(new ValidationError(nameof(ExpiresAt), "cannot be empty"));

    return validationErrors;
  }
}

public static class InvitationConstraints
{
  public const int EMAIL_MAX_LENGTH = 320;
  public const int NAME_MAX_LENGTH = 100;
  public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(7);
}

public static class InvitationMessages
{
  public const string DuplicateUserEmail = "A user with this email already exists.";
  public const string PendingInvitationExists = "An invitation for this email is already pending.";
  public const string OnlyPendingCanBeRevoked = "Only pending invitations can be revoked.";
  public const string OnlyPendingCanBeResent = "Only pending invitations can be resent.";
  public const string LinkNotValid = "This invitation link is not valid.";
  public const string NoLongerValid = "This invitation is no longer valid.";
  public const string AlreadyUsed = "This invitation has already been used.";
  public const string Expired = "This invitation has expired. Contact an administrator for a new invitation.";
}
