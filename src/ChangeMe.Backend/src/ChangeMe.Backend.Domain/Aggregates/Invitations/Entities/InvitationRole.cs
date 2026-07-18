namespace ChangeMe.Backend.Domain.Aggregates.Invitations.Entities;

public class InvitationRole
{
  private InvitationRole() { }

  public Guid InvitationId { get; private set; }
  public Guid RoleId { get; private set; }

  public static InvitationRole Create(Guid invitationId, Guid roleId) =>
    new()
    {
      InvitationId = invitationId,
      RoleId = roleId
    };
}
