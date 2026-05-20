namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record UserSessionDto(
  Guid Id,
  string DeviceBrowserLabel,
  string? IpAddress,
  bool IsPersistent,
  DateTime SignedInAt,
  DateTime LastActivityAt,
  bool IsCurrent);
