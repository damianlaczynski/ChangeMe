using Fido2NetLib;
using Fido2NetLib.Objects;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class PasskeyWebAuthnUtils
{
  public static bool HasUserVerification(AuthenticatorAssertionRawResponse response)
  {
    var authData = response.Response.AuthenticatorData;
    if (authData is null || authData.Length < 33)
      return false;

    const byte userVerifiedFlag = 0x04;
    return (authData[32] & userVerifiedFlag) == userVerifiedFlag;
  }
}
