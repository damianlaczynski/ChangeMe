namespace ChangeMe.Backend.Domain.Interfaces;

public interface IUserAccessor
{
  Guid? UserId { get; }
  Guid? SessionId { get; }
  bool HasPermission(string permissionCode);
}
