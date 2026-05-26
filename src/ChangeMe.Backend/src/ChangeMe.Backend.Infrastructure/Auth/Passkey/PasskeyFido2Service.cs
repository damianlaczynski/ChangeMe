using ChangeMe.Backend.Infrastructure.Persistence;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth.Passkey;

public interface IPasskeyFido2Service
{
  bool IsEnabled();

  CredentialCreateOptions BeginRegistration(
    Domain.Aggregates.Users.User user,
    IReadOnlyList<byte[]> existingCredentialIds,
    bool discoverable);

  Task<RegisteredPublicKeyCredential> CompleteRegistrationAsync(
    AuthenticatorAttestationRawResponse attestationResponse,
    CredentialCreateOptions options,
    CancellationToken cancellationToken);

  AssertionOptions BeginAuthentication(
    IReadOnlyList<byte[]>? allowCredentialIds,
    bool discoverable);

  Task<VerifyAssertionResult> CompleteAuthenticationAsync(
    AuthenticatorAssertionRawResponse assertionResponse,
    AssertionOptions options,
    byte[] storedPublicKey,
    uint storedSignCount,
    byte[] credentialId,
    CancellationToken cancellationToken);
}

public sealed class PasskeyFido2Service(
  IOptions<AuthOptions> authOptions,
  ApplicationDbContext context) : IPasskeyFido2Service
{
  public bool IsEnabled() => authOptions.Value.Passkeys.PasskeysAuthenticationEnabled;

  public CredentialCreateOptions BeginRegistration(
    Domain.Aggregates.Users.User user,
    IReadOnlyList<byte[]> existingCredentialIds,
    bool discoverable)
  {
    var fido2 = CreateFido2();
    var fidoUser = new Fido2User
    {
      Id = System.Text.Encoding.UTF8.GetBytes(user.Id.ToString("D")),
      Name = user.Email,
      DisplayName = user.DisplayLabel
    };

    var existing = existingCredentialIds
      .Select(id => new PublicKeyCredentialDescriptor(id))
      .ToList();

    var selection = BuildAuthenticatorSelection(discoverable);
    var attestation = MapAttestation(authOptions.Value.Passkeys.AttestationConveyance);

    return fido2.RequestNewCredential(new RequestNewCredentialParams
    {
      User = fidoUser,
      ExcludeCredentials = existing,
      AuthenticatorSelection = selection,
      AttestationPreference = attestation
    });
  }

  public async Task<RegisteredPublicKeyCredential> CompleteRegistrationAsync(
    AuthenticatorAttestationRawResponse attestationResponse,
    CredentialCreateOptions options,
    CancellationToken cancellationToken)
  {
    var fido2 = CreateFido2();
    return await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
    {
      AttestationResponse = attestationResponse,
      OriginalOptions = options,
      IsCredentialIdUniqueToUserCallback = IsCredentialIdUniqueAsync
    }, cancellationToken);

    async Task<bool> IsCredentialIdUniqueAsync(IsCredentialIdUniqueToUserParams args, CancellationToken ct)
    {
      var exists = await context.PasskeyCredentials
        .AsNoTracking()
        .AnyAsync(x => x.CredentialId == args.CredentialId, ct);
      return !exists;
    }
  }

  public AssertionOptions BeginAuthentication(
    IReadOnlyList<byte[]>? allowCredentialIds,
    bool discoverable)
  {
    var fido2 = CreateFido2();
    var uv = MapUserVerification(authOptions.Value.Passkeys.UserVerificationRequired);

    var allowed = allowCredentialIds is null || allowCredentialIds.Count == 0
      ? null
      : allowCredentialIds.Select(id => new PublicKeyCredentialDescriptor(id)).ToList();

    return fido2.GetAssertionOptions(new GetAssertionOptionsParams
    {
      AllowedCredentials = allowed,
      UserVerification = uv
    });
  }

  public async Task<VerifyAssertionResult> CompleteAuthenticationAsync(
    AuthenticatorAssertionRawResponse assertionResponse,
    AssertionOptions options,
    byte[] storedPublicKey,
    uint storedSignCount,
    byte[] credentialId,
    CancellationToken cancellationToken)
  {
    var fido2 = CreateFido2();
    return await fido2.MakeAssertionAsync(new MakeAssertionParams
    {
      AssertionResponse = assertionResponse,
      OriginalOptions = options,
      StoredPublicKey = storedPublicKey,
      StoredSignatureCounter = storedSignCount,
      IsUserHandleOwnerOfCredentialIdCallback = IsUserHandleOwnerOfCredentialIdAsync
    }, cancellationToken);

    Task<bool> IsUserHandleOwnerOfCredentialIdAsync(IsUserHandleOwnerOfCredentialIdParams args, CancellationToken ct)
    {
      if (args.CredentialId.Length != credentialId.Length)
        return Task.FromResult(false);

      return Task.FromResult(args.CredentialId.SequenceEqual(credentialId));
    }
  }

  private Fido2 CreateFido2()
  {
    var auth = authOptions.Value;
    var origin = auth.FrontendBaseUrl.TrimEnd('/');
    var rpId = ResolveRpId(auth);

    var config = new Fido2Configuration
    {
      ServerDomain = rpId,
      ServerName = auth.Passkeys.RelyingPartyDisplayName,
      Origins = new HashSet<string> { origin }
    };

    return new Fido2(config);
  }

  public static string ResolveRpId(AuthOptions auth)
  {
    if (!string.IsNullOrWhiteSpace(auth.Passkeys.RelyingPartyId))
      return auth.Passkeys.RelyingPartyId.Trim();

    return new Uri(auth.FrontendBaseUrl).Host;
  }

  private AuthenticatorSelection BuildAuthenticatorSelection(bool discoverable)
  {
    var passkeys = authOptions.Value.Passkeys;
    var attachment = MapAttachment(passkeys.AllowedAuthenticatorAttachment);

    return new AuthenticatorSelection
    {
      AuthenticatorAttachment = attachment,
      ResidentKey = discoverable
        ? ResidentKeyRequirement.Required
        : ResidentKeyRequirement.Discouraged,
      UserVerification = MapUserVerification(passkeys.UserVerificationRequired)
    };
  }

  private static AuthenticatorAttachment? MapAttachment(string value) =>
    value.Trim().ToUpperInvariant() switch
    {
      "PLATFORM" => AuthenticatorAttachment.Platform,
      "CROSS-PLATFORM" or "CROSSPLATFORM" => AuthenticatorAttachment.CrossPlatform,
      _ => null
    };

  private static UserVerificationRequirement MapUserVerification(bool required) =>
    required ? UserVerificationRequirement.Required : UserVerificationRequirement.Preferred;

  private static AttestationConveyancePreference MapAttestation(string value) =>
    value.Trim().ToUpperInvariant() switch
    {
      "INDIRECT" => AttestationConveyancePreference.Indirect,
      "DIRECT" => AttestationConveyancePreference.Direct,
      _ => AttestationConveyancePreference.None
    };
}
