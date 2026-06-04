namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class EmailChangeUtils
{
  public const string EmailChangeDisabledMessage = "Self-service email change is not available.";

  public const string InvitePendingBlocksEmailChangeMessage =
    "Complete your invitation before changing your email.";

  public const string EmailChangeAlreadyPendingMessage = "An email change is already pending.";

  public const string NoEmailChangePendingMessage = "No email change is pending.";

  public const string InvalidConfirmationLinkMessage =
    "This confirmation link is invalid or has expired.";

  public const string TargetEmailTakenMessage =
    "An account with this email already exists. Cancel the pending email change on My account.";

  public const string EmailChangeConfirmedMessage =
    "Email changed. Sign in with your new email address.";

  public const string ConfirmationWrongAccountMessage =
    "This confirmation link belongs to another account. Sign out and open the link again, or sign in as the account that requested the change.";
}
