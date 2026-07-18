using ChangeMe.Backend.Domain.Aggregates.Invitations;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Invitations.Dtos;
using ChangeMe.Backend.UseCases.Invitations.Utils;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;
using QueryGrid.Abstractions;
using QueryGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Invitations;

public class GetInvitationsQuery : IQuery<GridResult<InvitationListItemDto>>
{
  public GridQuery Grid { get; set; } = new();
}

public class GetInvitationsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetInvitationsQuery, GridResult<InvitationListItemDto>>
{
  public async ValueTask<Result<GridResult<InvitationListItemDto>>> Handle(
    GetInvitationsQuery query,
    CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.UsersInvite))
      return Result<GridResult<InvitationListItemDto>>.Forbidden(UsersUtils.PermissionDeniedMessage);

    await InvitationsUtils.ApplyExpiryTransitionsAsync(context, null, cancellationToken);

    var projected = context.Invitations
      .AsNoTracking()
      .Select(i => new InvitationListItemDto
      {
        Id = i.Id,
        Email = i.Email,
        Status = i.Status,
        RoleNames = i.Roles
          .Join(
            context.Roles,
            invitationRole => invitationRole.RoleId,
            role => role.Id,
            (_, role) => role.Name)
          .OrderBy(name => name)
          .ToList(),
        InvitedByUserId = i.InvitedByUserId,
        InvitedByName = context.Users
          .Where(u => u.Id == i.InvitedByUserId)
          .Select(u => u.FirstName + " " + u.LastName)
          .FirstOrDefault(),
        CreatedAt = i.CreatedAt,
        ExpiresAt = i.ExpiresAt,
        AcceptedAt = i.AcceptedAt
      });

    var grid = await projected.ToGridResultAsync(query.Grid, cancellationToken: cancellationToken);

    return Result.Success(grid);
  }
}

public sealed record GetInvitationByTokenQuery(string Token) : IQuery<InvitationAcceptanceDetailsDto>;

public class GetInvitationByTokenHandler(ApplicationDbContext context)
  : IQueryHandler<GetInvitationByTokenQuery, InvitationAcceptanceDetailsDto>
{
  public async ValueTask<Result<InvitationAcceptanceDetailsDto>> Handle(
    GetInvitationByTokenQuery query,
    CancellationToken cancellationToken)
  {
    var tokenHash = InvitationTokenGenerator.HashToken(query.Token);
    var invitation = await InvitationsUtils.FindByTokenHashAsync(context, tokenHash, cancellationToken);
    if (invitation is null)
      return Result<InvitationAcceptanceDetailsDto>.Error(InvitationMessages.LinkNotValid);

    await InvitationsUtils.ApplyExpiryTransitionsAsync(context, [invitation.Id], cancellationToken);

    var validation = invitation.ValidateForOpen(tokenHash, DateTime.UtcNow);
    if (!validation.IsSuccess)
      return validation.Map();

    return Result.Success(new InvitationAcceptanceDetailsDto
    {
      Email = invitation.Email,
      FirstName = invitation.FirstName,
      LastName = invitation.LastName
    });
  }
}

public sealed record CreateInvitationCommand(
  string Email,
  string? FirstName,
  string? LastName,
  IReadOnlyList<Guid>? RoleIds) : ICommand<CreateInvitationResultDto>;

public class CreateInvitationHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IEmailService emailService,
  IOptions<AuthOptions> authOptions) : ICommandHandler<CreateInvitationCommand, CreateInvitationResultDto>
{
  public async ValueTask<Result<CreateInvitationResultDto>> Handle(
    CreateInvitationCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    if (!userAccessor.HasPermission(PermissionCodes.UsersInvite))
      return Result<CreateInvitationResultDto>.Forbidden(UsersUtils.PermissionDeniedMessage);

    var normalizedEmail = User.NormalizeEmail(command.Email);
    if (await InvitationsUtils.HasActiveUserWithEmailAsync(context, normalizedEmail, cancellationToken))
      return Result<CreateInvitationResultDto>.Conflict(InvitationMessages.DuplicateUserEmail);

    if (await InvitationsUtils.HasPendingInvitationForEmailAsync(context, normalizedEmail, cancellationToken))
      return Result<CreateInvitationResultDto>.Conflict(InvitationMessages.PendingInvitationExists);

    var roleIds = await InvitationsUtils.ResolveRoleIdsForCreateAsync(
      context,
      userAccessor,
      command.RoleIds,
      cancellationToken);

    if (roleIds.Count == 0)
      return Result<CreateInvitationResultDto>.Invalid(new ValidationError(nameof(command.RoleIds), UsersUtils.AtLeastOneRoleRequiredMessage));

    var existingRoleCount = await context.Roles.CountAsync(r => roleIds.Contains(r.Id), cancellationToken);
    if (existingRoleCount != roleIds.Count)
      return Result<CreateInvitationResultDto>.NotFound();

    var token = InvitationTokenGenerator.CreateToken();
    var tokenHash = InvitationTokenGenerator.HashToken(token);
    var utcNow = DateTime.UtcNow;
    var expiresAt = InvitationsUtils.CalculateExpiresAt(utcNow);

    var invitationResult = Invitation.Create(
      command.Email,
      command.FirstName,
      command.LastName,
      roleIds,
      actorUserId,
      tokenHash,
      expiresAt);

    if (!invitationResult.IsSuccess)
      return invitationResult.Map();

    await context.Invitations.AddAsync(invitationResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var emailResult = await InvitationsUtils.SendInvitationEmailAsync(
      emailService,
      authOptions,
      invitationResult.Value.Email,
      token,
      expiresAt);

    if (!emailResult.IsSuccess)
      return emailResult.Map();

    return Result.Created(
      new CreateInvitationResultDto { Id = invitationResult.Value.Id },
      $"/invitations/{invitationResult.Value.Id}");
  }
}

public sealed record ResendInvitationCommand(Guid Id) : ICommand<Guid>;

public class ResendInvitationHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IEmailService emailService,
  IOptions<AuthOptions> authOptions) : ICommandHandler<ResendInvitationCommand, Guid>
{
  public async ValueTask<Result<Guid>> Handle(ResendInvitationCommand command, CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.UsersInvite))
      return Result<Guid>.Forbidden(UsersUtils.PermissionDeniedMessage);

    var invitation = await context.Invitations.FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);
    if (invitation is null)
      return Result<Guid>.NotFound();

    invitation.ApplyExpiryIfNeeded(DateTime.UtcNow);
    if (invitation.Status != InvitationStatus.PENDING)
      return Result<Guid>.Error(InvitationMessages.OnlyPendingCanBeResent);

    var token = InvitationTokenGenerator.CreateToken();
    var tokenHash = InvitationTokenGenerator.HashToken(token);
    var expiresAt = InvitationsUtils.CalculateExpiresAt(DateTime.UtcNow);

    var resendResult = invitation.Resend(tokenHash, expiresAt);
    if (!resendResult.IsSuccess)
      return resendResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    var emailResult = await InvitationsUtils.SendInvitationEmailAsync(
      emailService,
      authOptions,
      invitation.Email,
      token,
      expiresAt);

    if (!emailResult.IsSuccess)
      return emailResult.Map();

    return Result.Success(invitation.Id);
  }
}

public sealed record RevokeInvitationCommand(Guid Id) : ICommand<Guid>;

public class RevokeInvitationHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<RevokeInvitationCommand, Guid>
{
  public async ValueTask<Result<Guid>> Handle(RevokeInvitationCommand command, CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.UsersInvite))
      return Result<Guid>.Forbidden(UsersUtils.PermissionDeniedMessage);

    var invitation = await context.Invitations.FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);
    if (invitation is null)
      return Result<Guid>.NotFound();

    var revokeResult = invitation.Revoke();
    if (!revokeResult.IsSuccess)
      return revokeResult.Map();

    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(invitation.Id);
  }
}

public sealed record AcceptInvitationCommand(
  string Token,
  string FirstName,
  string LastName,
  string Password) : ICommand<Guid>;

public class AcceptInvitationHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher) : ICommandHandler<AcceptInvitationCommand, Guid>
{
  public async ValueTask<Result<Guid>> Handle(AcceptInvitationCommand command, CancellationToken cancellationToken)
  {
    var tokenHash = InvitationTokenGenerator.HashToken(command.Token);
    var invitation = await InvitationsUtils.FindByTokenHashAsync(context, tokenHash, cancellationToken);
    if (invitation is null)
      return Result<Guid>.Error(InvitationMessages.LinkNotValid);

    await InvitationsUtils.ApplyExpiryTransitionsAsync(context, [invitation.Id], cancellationToken);

    var validation = invitation.ValidateForOpen(tokenHash, DateTime.UtcNow);
    if (!validation.IsSuccess)
      return validation.Map();

    var normalizedEmail = invitation.NormalizedEmail;
    if (await InvitationsUtils.HasActiveUserWithEmailAsync(context, normalizedEmail, cancellationToken))
      return Result<Guid>.Conflict(InvitationMessages.DuplicateUserEmail);

    var passwordHash = passwordHasher.HashPassword(command.Password);
    var createUserResult = User.Create(command.FirstName, command.LastName, invitation.Email, passwordHash);
    if (!createUserResult.IsSuccess)
      return createUserResult.Map();

    var user = createUserResult.Value;
    var roleIds = invitation.Roles.Select(r => r.RoleId).Distinct().ToList();
    var roleResult = user.ReplaceRoles(roleIds);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var acceptedAt = DateTime.UtcNow;
    var acceptResult = invitation.MarkAccepted(acceptedAt);
    if (!acceptResult.IsSuccess)
      return acceptResult.Map();

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(user.Id);
  }
}
