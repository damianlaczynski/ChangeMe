namespace ChangeMe.Backend.UseCases.Users.Dtos;

public sealed record UserInvitationInfoDto(
  DateTime LastSentAtUtc,
  DateTime ExpiresAtUtc,
  bool IsLinkExpired);
