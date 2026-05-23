using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Interfaces;
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
    var user = User.CreateWithPassword("Ada", "Lovelace", "ada@example.com", "hash").Value;
    const string plainToken = "token+with/special==";

    await sut.SendPasswordResetRequestedAsync(user, plainToken);

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
    var user = User.CreateInvited("ada@example.com").Value;

    await sut.SendAccountInvitationAsync(user, "invite-token");

    Assert.Single(emailService.Messages);
    var message = emailService.Messages[0];
    Assert.Contains("https://app.example.com/accept-invitation?token=invite-token", message.Body);
  }

  private sealed class RecordingEmailService : IEmailService
  {
    public List<RecordedEmail> Messages { get; } = [];

    public Task<Result> SendEmailAsync(string to, string subject, string body)
    {
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
