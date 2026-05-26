using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class AuthEmailServiceTests
{
  private readonly RecordingEmailService emailService = new();
  private readonly AuthEmailService sut;

  public AuthEmailServiceTests()
  {
    sut = new AuthEmailService(
      emailService,
      Options.Create(new AuthOptions
      {
        FrontendBaseUrl = "https://app.example.com/"
      }),
      NullLogger<AuthEmailService>.Instance);
  }

  [Fact]
  public async Task SendPasswordResetRequestedAsync_ShouldIncludeEncodedTokenLink()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = User.CreateWithPassword("Ada", "Lovelace", "ada@example.com", "hash").Value;
    const string plainToken = "token+with/special==";

    await sut.SendPasswordResetRequestedAsync(user, plainToken, cancellationToken);

    Assert.Single(emailService.Messages);
    var message = emailService.Messages[0];
    Assert.Equal(user.Email, message.To);
    Assert.Equal("Reset your ChangeMe password", message.Subject);
    Assert.Contains("https://app.example.com/reset-password?token=", message.Body);
    Assert.Contains(Uri.EscapeDataString(plainToken), message.Body);
  }

  [Fact]
  public async Task SendAccountInvitationAsync_ShouldTargetAcceptInvitationRoute()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = User.CreateInvited("ada@example.com").Value;

    await sut.SendAccountInvitationAsync(user, "invite-token", cancellationToken);

    Assert.Single(emailService.Messages);
    var message = emailService.Messages[0];
    Assert.Contains("https://app.example.com/accept-invitation?token=invite-token", message.Body);
  }

  [Fact]
  public async Task SendPasskeyAddedAsync_ShouldIncludeAccountEmailEventTimeAndPasskeyName()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = User.CreateWithPassword("Ada", "Lovelace", "ada@example.com", "hash").Value;

    await sut.SendPasskeyAddedAsync(user, "Work laptop", cancellationToken);

    Assert.Single(emailService.Messages);
    var message = emailService.Messages[0];
    Assert.Equal("Passkey added to your account", message.Subject);
    Assert.Contains("Account: ada@example.com", message.Body);
    Assert.Contains("Event time (UTC):", message.Body);
    Assert.Contains("Passkey: Work laptop", message.Body);
    Assert.Contains(
      "If you did not perform this action, contact your administrator immediately.",
      message.Body);
  }

  [Fact]
  public async Task SendPasskeysResetByAdminAsync_ShouldIncludeAccountEmailAndEventTime()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = User.CreateWithPassword("Ada", "Lovelace", "ada@example.com", "hash").Value;

    await sut.SendPasskeysResetByAdminAsync(user, cancellationToken);

    Assert.Single(emailService.Messages);
    var message = emailService.Messages[0];
    Assert.Equal("Passkeys reset on your account", message.Subject);
    Assert.Contains("Account: ada@example.com", message.Body);
    Assert.Contains("Event time (UTC):", message.Body);
    Assert.DoesNotContain("Passkey:", message.Body);
  }

  [Fact]
  public async Task SendPasskeyRemovedAsync_ShouldIncludeAccountEmailEventTimeAndPasskeyName()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = User.CreateWithPassword("Ada", "Lovelace", "ada@example.com", "hash").Value;

    await sut.SendPasskeyRemovedAsync(user, "Work laptop", cancellationToken);

    Assert.Single(emailService.Messages);
    var message = emailService.Messages[0];
    Assert.Equal("Passkey removed from your account", message.Subject);
    Assert.Contains("Account: ada@example.com", message.Body);
    Assert.Contains("Event time (UTC):", message.Body);
    Assert.Contains("Passkey: Work laptop", message.Body);
    Assert.Contains(
      "If you did not perform this action, contact your administrator immediately.",
      message.Body);
  }

  [Fact]
  public async Task SendAsync_WhenDeliveryFails_ReturnsUserFacingMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    emailService.ShouldFail = true;
    var user = User.CreateInvited("ada@example.com").Value;

    var result = await sut.SendVerifyEmailAsync(user, "verify-token", cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains("The email could not be sent. Please try again.", result.Errors);
  }

  private sealed class RecordingEmailService : IEmailService
  {
    public List<RecordedEmail> Messages { get; } = [];
    public bool ShouldFail { get; set; }

    public Task<Result> SendEmailAsync(string to, string subject, string body)
    {
      if (ShouldFail)
        return Task.FromResult(Result.Error());

      Messages.Add(new RecordedEmail(to, subject, body));
      return Task.FromResult(Result.Success());
    }

    public Task<Result> SendEmailToManyAsync(IEnumerable<string> recipients, string subject, string body)
    {
      Messages.Add(new RecordedEmail(string.Join(',', recipients), subject, body));
      return Task.FromResult(Result.Success());
    }
  }

  private sealed record RecordedEmail(string To, string Subject, string Body);
}
