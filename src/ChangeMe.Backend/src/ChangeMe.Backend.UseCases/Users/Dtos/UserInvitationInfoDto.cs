namespace ChangeMe.Backend.UseCases.Users.Dtos;

public sealed record UserInvitationInfoDto(
  DateTime LastSentAtUtc,
  int SentCount,
  DateTime ExpiresAtUtc);
