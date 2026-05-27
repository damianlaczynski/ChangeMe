using System.Text;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class PasskeyTestResponses
{
  internal static readonly byte[] DefaultCredentialId =
    Encoding.UTF8.GetBytes("integration-passkey-credential");

  internal static readonly byte[] AlternateCredentialId =
    Encoding.UTF8.GetBytes("integration-passkey-alt-cred");

  internal static object CreateAttestationResponse(byte[] credentialId) =>
    new
    {
      id = Base64UrlEncode(credentialId),
      rawId = Base64UrlEncode(credentialId),
      type = "public-key",
      response = new
      {
        clientDataJSON = Base64UrlEncode("{\"type\":\"webauthn.create\"}"u8.ToArray()),
        attestationObject = Base64UrlEncode([0x01, 0x02, 0x03])
      }
    };

  internal static object CreateAssertionResponse(byte[] credentialId, bool userVerification = true)
  {
    var authenticatorData = new byte[37];
    if (userVerification)
      authenticatorData[32] = 0x04;

    return new
    {
      id = Base64UrlEncode(credentialId),
      rawId = Base64UrlEncode(credentialId),
      type = "public-key",
      response = new
      {
        clientDataJSON = Base64UrlEncode("{\"type\":\"webauthn.get\"}"u8.ToArray()),
        authenticatorData = Base64UrlEncode(authenticatorData),
        signature = Base64UrlEncode([0x01, 0x02, 0x03])
      }
    };
  }

  private static string Base64UrlEncode(byte[] data) =>
    Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
