namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface ITwoFactorSecretProtector
{
  string Protect(string plaintextSecret);

  string Unprotect(string ciphertext);
}
