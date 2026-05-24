namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record PasskeyCeremonyBeginResponseDto(Guid CeremonyId, object Options);

public sealed record PasskeySignInBeginCommandDto(string? Email);

public sealed record PasskeySignInCompleteCommandDto(
  Guid CeremonyId,
  object AttestationResponse);

public sealed record PasskeyRegisterCompleteCommandDto(
  Guid CeremonyId,
  object AttestationResponse,
  string Name);

public sealed record PasskeyRenameCommandDto(string Name);

public sealed record MyAccountPasskeyDto(
  Guid Id,
  string Name,
  DateTime CreatedAtUtc,
  DateTime? LastUsedAtUtc,
  string AuthenticatorType,
  bool BackupEligible,
  bool BackupState);

public sealed record PasskeySettingsDto
{
  public bool PasskeysAuthenticationEnabled { get; init; }
  public bool PasskeysAuthenticationRequired { get; init; }
  public bool PasskeySatisfiesTwoFactor { get; init; }
  public bool DiscoverablePasskeySignInOnLogin { get; init; }
  public string RelyingPartyId { get; init; } = string.Empty;
  public string RelyingPartyDisplayName { get; init; } = "ChangeMe";
  public int MaximumPasskeysPerUser { get; init; } = 10;
}
