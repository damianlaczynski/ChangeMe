using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record InviteUserCommand(
  string FirstName,
  string LastName,
  string Email,
  IReadOnlyList<Guid> RoleIds) : ICommand<UserDetailsDto>;

public class InviteUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  UserInvitationService invitationService) : ICommandHandler<InviteUserCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(InviteUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var emailExists = await context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (emailExists)
      return Result<UserDetailsDto>.Conflict(UsersUtils.DuplicateEmailMessage);

    var firstName = string.IsNullOrWhiteSpace(command.FirstName) ? null : command.FirstName;
    var lastName = string.IsNullOrWhiteSpace(command.LastName) ? null : command.LastName;

    var inviteUserResult = User.CreateInvited(command.Email, firstName, lastName);
    if (!inviteUserResult.IsSuccess)
      return inviteUserResult.Map();

    var user = inviteUserResult.Value;

    var distinctRoleIds = command.RoleIds.Distinct().ToList();
    var existingRoleCount = await context.Roles
      .CountAsync(x => distinctRoleIds.Contains(x.Id), cancellationToken);

    if (existingRoleCount != distinctRoleIds.Count)
      return Result<UserDetailsDto>.NotFound();

    var roleResult = user.ReplaceRoles(distinctRoleIds);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    await context.Users.AddAsync(user, cancellationToken);

    var invitationResult = await invitationService.SendInvitationAsync(user, cancellationToken);
    if (!invitationResult.IsSuccess)
      return invitationResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    var invitedUserResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    if (!invitedUserResult.IsSuccess)
      return invitedUserResult.Map();

    return Result.Created(invitedUserResult.Value, $"/users/{invitedUserResult.Value.Id}");
  }
}
