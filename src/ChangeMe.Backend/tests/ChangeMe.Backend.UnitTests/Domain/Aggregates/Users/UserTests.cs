using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.UnitTests;

public sealed class UserTests
{
  [Fact]
  public void CreateWithPassword_WhenInputIsValid_ShouldTrimValuesAndSetAccountFields()
  {
    var result = User.CreateWithPassword("  John  ", "  Doe  ", " Test@Example.com ", "  hashed-password  ");

    Assert.True(result.IsSuccess);
    var user = result.Value;
    Assert.Equal("John", user.FirstName);
    Assert.Equal("Doe", user.LastName);
    Assert.Equal("Test@Example.com", user.Email);
    Assert.Equal("TEST@EXAMPLE.COM", user.NormalizedEmail);
    Assert.Equal("hashed-password", user.PasswordHash);
    Assert.True(user.HasPasswordSet);
    Assert.True(user.EmailVerified);
    Assert.NotNull(user.EmailVerifiedAt);
    Assert.NotNull(user.PasswordLastChangedAt);
    Assert.False(user.Deactivated);
    Assert.Equal("John Doe (Test@Example.com)", user.DisplayLabel);
  }

  [Fact]
  public void CreateInvited_WhenNamesAreOmitted_ShouldAllowPendingProfile()
  {
    var result = User.CreateInvited("invite@example.com");

    Assert.True(result.IsSuccess);
    var user = result.Value;
    Assert.Equal(string.Empty, user.FirstName);
    Assert.Equal(string.Empty, user.LastName);
    Assert.False(user.HasPasswordSet);
    Assert.Equal(string.Empty, user.PasswordHash);
    Assert.Equal("invite@example.com", user.DisplayLabel);
    Assert.True(user.EmailVerified);
  }

  [Fact]
  public void DisplayLabel_WhenProfileIsComplete_ShouldIncludeNameAndEmail()
  {
    var user = User.CreateWithPassword("Jan", "Kowalski", "jan@example.com", "hash").Value;

    Assert.Equal("Jan Kowalski (jan@example.com)", user.DisplayLabel);
  }

  [Fact]
  public void DisplayLabel_WhenOnlyFirstNameIsSet_ShouldIncludeFirstNameAndEmail()
  {
    var user = User.CreateInvited("jan@example.com", "Jan").Value;

    Assert.Equal("Jan (jan@example.com)", user.DisplayLabel);
  }

  [Fact]
  public void CreateInvited_WhenOptionalNamesProvided_ShouldTrimValues()
  {
    var result = User.CreateInvited("invite@example.com", "  Ada  ", "  Lovelace  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("Ada", result.Value.FirstName);
    Assert.Equal("Lovelace", result.Value.LastName);
    Assert.Equal("Ada Lovelace (invite@example.com)", result.Value.DisplayLabel);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("invalid-email")]
  public void CreateWithPassword_WhenEmailIsInvalid_ShouldReturnInvalidResult(string email)
  {
    var result = User.CreateWithPassword("John", "Doe", email, "hashed-password");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void CreateWithPassword_WhenNamesAreMissing_ShouldReturnInvalidResult()
  {
    var result = User.CreateWithPassword("", " ", "john@example.com", "hashed-password");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void CreateWithPassword_WhenPasswordHashIsMissing_ShouldReturnInvalidResult()
  {
    var result = User.CreateWithPassword("John", "Doe", "john@example.com", " ");

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    Assert.Contains(result.ValidationErrors, error => error.Identifier == nameof(User.PasswordHash));
  }

  [Fact]
  public void Deactivate_WhenCalled_ShouldSetDeactivatedAt()
  {
    var user = User.CreateWithPassword("John", "Doe", "john@example.com", "hash").Value;

    user.Deactivate();

    Assert.True(user.Deactivated);
    Assert.NotNull(user.DeactivatedAt);
    Assert.False(user.IsActive);
  }

  [Fact]
  public void Activate_WhenCalled_ShouldClearDeactivatedAt()
  {
    var user = User.CreateWithPassword("John", "Doe", "john@example.com", "hash").Value;
    user.Deactivate();

    user.Activate();

    Assert.False(user.Deactivated);
    Assert.Null(user.DeactivatedAt);
  }

  [Theory]
  [InlineData(" Test@Example.com ", "TEST@EXAMPLE.COM")]
  [InlineData("user@example.com", "USER@EXAMPLE.COM")]
  public void NormalizeEmail_WhenCalled_ShouldTrimAndUppercaseValue(string email, string expected)
  {
    var result = User.NormalizeEmail(email);

    Assert.Equal(expected, result);
  }

  [Fact]
  public void CompleteInvitationViaExternalSignIn_WhenInvitationPending_ShouldClearInvitationAndFillProfile()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("invite@example.com").Value;
    user.RecordInvitationIssued(utcNow);

    var result = user.CompleteInvitationViaExternalSignIn("Oidc", "User", utcNow);

    Assert.True(result.IsSuccess);
    Assert.False(user.HasPendingInvitation);
    Assert.True(user.EmailVerified);
    Assert.Equal("Oidc", user.FirstName);
    Assert.Equal("User", user.LastName);
  }

  [Fact]
  public void CompleteInvitationViaExternalSignIn_WhenProfileAlreadyComplete_ShouldKeepExistingNames()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("invite@example.com", "Admin", "Created").Value;
    user.RecordInvitationIssued(utcNow);

    var result = user.CompleteInvitationViaExternalSignIn("Oidc", "User", utcNow);

    Assert.True(result.IsSuccess);
    Assert.Equal("Admin", user.FirstName);
    Assert.Equal("Created", user.LastName);
  }

  [Fact]
  public void CompleteInvitationViaExternalSignIn_WhenPasswordAlreadySet_ShouldFail()
  {
    var user = User.CreateWithPassword("John", "Doe", "john@example.com", "hash").Value;

    var result = user.CompleteInvitationViaExternalSignIn("Oidc", "User", DateTime.UtcNow);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void CompleteInvitationViaExternalSignIn_WhenNoInvitationPending_ShouldFail()
  {
    var user = User.CreateInvited("invite@example.com").Value;

    var result = user.CompleteInvitationViaExternalSignIn("Oidc", "User", DateTime.UtcNow);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void RecordInvitationIssued_WhenResent_ShouldRevokePreviousAndIncrementCount()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("invite@example.com").Value;

    user.RecordInvitationIssued(utcNow);
    user.RecordInvitationIssued(utcNow.AddMinutes(5));

    var summary = user.GetPendingInvitationSummary();

    Assert.NotNull(summary);
    Assert.Equal(2, summary.SentCount);
    Assert.Equal(utcNow.AddMinutes(5), summary.LastSentAtUtc);
    Assert.Equal(1, user.AccountInvitations.Count(x => x.IsPending));
    Assert.Equal(1, user.AccountInvitations.Count(x => !x.IsPending));
  }

  [Fact]
  public void AcceptPendingInvitation_WhenPendingExists_ShouldMarkAccepted()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("invite@example.com").Value;
    user.RecordInvitationIssued(utcNow);

    var result = user.AcceptPendingInvitation(utcNow);

    Assert.True(result.IsSuccess);
    Assert.Null(user.GetPendingInvitationSummary());
    Assert.All(user.AccountInvitations, invitation => Assert.False(invitation.IsPending));
  }
}
