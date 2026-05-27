using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Auth.Passkey;
using ChangeMe.Backend.Infrastructure.Persistence;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.IntegrationTests.Support.Fakes;

/// <summary>
/// Bypasses real WebAuthn crypto in integration tests while preserving ceremony option generation.
/// </summary>
internal sealed class FakePasskeyFido2Service(
  IOptions<AuthOptions> authOptions,
  ApplicationDbContext context) : IPasskeyFido2Service
{
  internal static readonly byte[] TestPublicKey = Enumerable.Repeat((byte)0xAB, 32).ToArray();

  private readonly PasskeyFido2Service inner = new(authOptions, context);

  public bool IsEnabled() => inner.IsEnabled();

  public CredentialCreateOptions BeginRegistration(
    Domain.Aggregates.Users.User user,
    IReadOnlyList<byte[]> existingCredentialIds,
    bool discoverable) =>
    inner.BeginRegistration(user, existingCredentialIds, discoverable);

  public AssertionOptions BeginAuthentication(
    IReadOnlyList<byte[]>? allowCredentialIds,
    bool discoverable) =>
    inner.BeginAuthentication(allowCredentialIds, discoverable);

  public Task<RegisteredPublicKeyCredential> CompleteRegistrationAsync(
    AuthenticatorAttestationRawResponse attestationResponse,
    CredentialCreateOptions options,
    CancellationToken cancellationToken)
  {
    var credential = new RegisteredPublicKeyCredential
    {
      Id = attestationResponse.RawId,
      PublicKey = TestPublicKey,
      SignCount = 0,
      AaGuid = Guid.Empty,
      Transports = [AuthenticatorTransport.Internal],
      IsBackupEligible = false,
      IsBackedUp = false
    };

    return Task.FromResult(credential);
  }

  public Task<VerifyAssertionResult> CompleteAuthenticationAsync(
    AuthenticatorAssertionRawResponse assertionResponse,
    AssertionOptions options,
    byte[] storedPublicKey,
    uint storedSignCount,
    byte[] credentialId,
    CancellationToken cancellationToken)
  {
    if (!assertionResponse.RawId.SequenceEqual(credentialId))
    {
      throw new Fido2VerificationException("Credential id mismatch.");
    }

    return Task.FromResult(new VerifyAssertionResult
    {
      SignCount = storedSignCount + 1
    });
  }
}
