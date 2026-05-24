using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using Fido2NetLib;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class PasskeyCeremonyUtils
{
  public static string SerializeCreateOptions(CredentialCreateOptions options) =>
    options.ToJson();

  public static CredentialCreateOptions DeserializeCreateOptions(string json) =>
    CredentialCreateOptions.FromJson(json);

  public static string SerializeAssertionOptions(AssertionOptions options) =>
    options.ToJson();

  public static AssertionOptions DeserializeAssertionOptions(string json) =>
    AssertionOptions.FromJson(json);

  public static async Task<Result<WebAuthnCeremonyPending>> StoreCeremonyAsync(
    ApplicationDbContext context,
    WebAuthnCeremonyType type,
    string optionsJson,
    DateTime expiresAtUtc,
    CancellationToken cancellationToken,
    Guid? userId = null,
    string? normalizedEmail = null)
  {
    var ceremonyResult = WebAuthnCeremonyPending.Create(
      type,
      optionsJson,
      expiresAtUtc,
      userId,
      normalizedEmail);

    if (!ceremonyResult.IsSuccess)
      return ceremonyResult.Map();

    await context.WebAuthnCeremonyPending.AddAsync(ceremonyResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return ceremonyResult;
  }

  public static async Task<WebAuthnCeremonyPending?> LoadCeremonyAsync(
    ApplicationDbContext context,
    Guid ceremonyId,
    WebAuthnCeremonyType expectedType,
    CancellationToken cancellationToken)
  {
    var ceremony = await context.WebAuthnCeremonyPending
      .FirstOrDefaultAsync(x => x.Id == ceremonyId, cancellationToken);

    if (ceremony is null || ceremony.CeremonyType != expectedType)
      return null;

    return ceremony;
  }
}
