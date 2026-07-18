using ChangeMe.Backend.Domain.Aggregates.Invitations;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Invitations;
using ChangeMe.Backend.UseCases.Invitations.Dtos;
using ChangeMe.Backend.Web.Validation;
using QueryGrid.Abstractions;

namespace ChangeMe.Backend.Web.Invitations;

public class GetInvitations(IMediator mediator)
  : BaseEndpoint<GetInvitationsQuery, GridResult<InvitationListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersInvite);
    Get("/invitations");
    Summary(s => s.Summary = "Get invitations");
  }
}

public class GetInvitationByToken(IMediator mediator)
  : BaseEndpoint<GetInvitationByTokenQuery, InvitationAcceptanceDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    AllowAnonymous();
    Get("/invitations/accept/{token}");
    Summary(s => s.Summary = "Get invitation acceptance details");
  }
}

public class CreateInvitation(IMediator mediator)
  : BaseEndpoint<CreateInvitationCommand, CreateInvitationResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersInvite);
    Post("/invitations");
    Summary(s => s.Summary = "Create invitation");
  }
}

public class ResendInvitation(IMediator mediator) : BaseEndpoint<ResendInvitationCommand, Guid>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersInvite);
    Post("/invitations/{id}/resend");
    Summary(s => s.Summary = "Resend invitation");
  }
}

public class RevokeInvitation(IMediator mediator) : BaseEndpoint<RevokeInvitationCommand, Guid>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersInvite);
    Post("/invitations/{id}/revoke");
    Summary(s => s.Summary = "Revoke invitation");
  }
}

public class AcceptInvitation(IMediator mediator) : BaseEndpoint<AcceptInvitationCommand, Guid>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    AllowAnonymous();
    Post("/invitations/accept/{token}");
    Summary(s => s.Summary = "Accept invitation");
  }
}

public sealed class CreateInvitationCommandValidator : Validator<CreateInvitationCommand>
{
  public CreateInvitationCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(InvitationConstraints.EMAIL_MAX_LENGTH);

    RuleFor(x => x.FirstName)
      .MaximumLength(InvitationConstraints.NAME_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

    RuleFor(x => x.LastName)
      .MaximumLength(InvitationConstraints.NAME_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.LastName));
  }
}

public sealed class AcceptInvitationCommandValidator : Validator<AcceptInvitationCommand>
{
  public AcceptInvitationCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.Token)
      .NotEmpty();

    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(InvitationConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(InvitationConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Password)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}

public sealed class ResendInvitationCommandValidator : Validator<ResendInvitationCommand>
{
  public ResendInvitationCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

public sealed class RevokeInvitationCommandValidator : Validator<RevokeInvitationCommand>
{
  public RevokeInvitationCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

public sealed class GetInvitationByTokenQueryValidator : Validator<GetInvitationByTokenQuery>
{
  public GetInvitationByTokenQueryValidator()
  {
    RuleFor(x => x.Token).NotEmpty();
  }
}
