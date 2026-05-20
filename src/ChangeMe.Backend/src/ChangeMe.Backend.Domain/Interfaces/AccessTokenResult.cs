namespace ChangeMe.Backend.Domain.Interfaces;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
