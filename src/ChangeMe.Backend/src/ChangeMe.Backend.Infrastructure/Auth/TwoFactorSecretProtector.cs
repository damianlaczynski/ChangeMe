using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class TwoFactorSecretProtector(IDataProtectionProvider dataProtectionProvider)
  : ITwoFactorSecretProtector
{
  private const string ProtectorPurpose = "ChangeMe.TwoFactor.Secret.v1";

  public string Protect(string plaintextSecret)
  {
    var protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    return protector.Protect(plaintextSecret);
  }

  public string Unprotect(string ciphertext)
  {
    var protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    return protector.Unprotect(ciphertext);
  }
}
