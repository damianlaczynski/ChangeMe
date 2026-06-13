using ChangeMe.Backend.Domain.Aggregates.Projects;
using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetProjects(IMediator mediator) : BaseEndpoint<GetProjectsQuery, PaginationResult<ProjectListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Get("/projects");
    Summary(s => s.Summary = "Get projects");
  }
}

public class GetProjectById(IMediator mediator) : BaseEndpoint<GetProjectByIdQuery, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Get("/projects/{Id}");
    Summary(s => s.Summary = "Get project by id");
  }
}

public class GetProjectOverview(IMediator mediator) : BaseEndpoint<GetProjectOverviewQuery, ProjectOverviewDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Get("/projects/{Id}/overview");
    Summary(s => s.Summary = "Get project overview");
  }
}

public class GetProjectsForSelection(IMediator mediator)
  : BaseEndpointWithoutRequest<GetProjectsForSelectionQuery, IReadOnlyList<ProjectSelectionItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Get("/projects/for-selection");
    Summary(s => s.Summary = "Get projects for selection");
  }

  protected override GetProjectsForSelectionQuery CreateRequest() => new();
}

public class CreateProject(IMediator mediator) : BaseEndpoint<CreateProjectCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsManage);
    Post("/projects");
    Summary(s => s.Summary = "Create project");
  }
}

public sealed class CreateProjectCommandValidator : Validator<CreateProjectCommand>
{
  public CreateProjectCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MinimumLength(ProjectConstraints.NAME_MIN_LENGTH)
      .MaximumLength(ProjectConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Key)
      .NotEmpty()
      .Must(ProjectConstraints.IsValidKey)
      .WithMessage($"Key must be {ProjectConstraints.KEY_MIN_LENGTH}-{ProjectConstraints.KEY_MAX_LENGTH} uppercase letters or digits");

    RuleFor(x => x.Description)
      .MaximumLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));

    RuleFor(x => x.Visibility)
      .IsInEnum();

    RuleFor(x => x.Color)
      .Must(c => string.IsNullOrWhiteSpace(c) || ProjectConstraints.IsValidColor(c.Trim()))
      .When(x => !string.IsNullOrWhiteSpace(x.Color));
  }
}

public class UpdateProject(IMediator mediator) : BaseEndpoint<UpdateProjectCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Put("/projects/{Id}");
    Summary(s => s.Summary = "Update project");
  }
}

public sealed class UpdateProjectCommandValidator : Validator<UpdateProjectCommand>
{
  public UpdateProjectCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();

    RuleFor(x => x.Name)
      .NotEmpty()
      .MinimumLength(ProjectConstraints.NAME_MIN_LENGTH)
      .MaximumLength(ProjectConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Key)
      .NotEmpty()
      .Must(ProjectConstraints.IsValidKey)
      .WithMessage($"Key must be {ProjectConstraints.KEY_MIN_LENGTH}-{ProjectConstraints.KEY_MAX_LENGTH} uppercase letters or digits");

    RuleFor(x => x.Description)
      .MaximumLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));

    RuleFor(x => x.Visibility)
      .IsInEnum();

    RuleFor(x => x.Status)
      .IsInEnum();

    RuleFor(x => x.Color)
      .Must(c => string.IsNullOrWhiteSpace(c) || ProjectConstraints.IsValidColor(c.Trim()))
      .When(x => !string.IsNullOrWhiteSpace(x.Color));
  }
}

public class DeleteProject(IMediator mediator) : BaseEndpoint<DeleteProjectCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Delete("/projects/{Id}");
    Summary(s => s.Summary = "Delete project");
  }
}

public class AddProjectMember(IMediator mediator) : BaseEndpoint<AddProjectMemberCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Post("/projects/{ProjectId}/members");
    Summary(s => s.Summary = "Add project member");
  }
}

public sealed class AddProjectMemberCommandValidator : Validator<AddProjectMemberCommand>
{
  public AddProjectMemberCommandValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Role).IsInEnum();
  }
}

public class RemoveProjectMember(IMediator mediator) : BaseEndpoint<RemoveProjectMemberCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Delete("/projects/{ProjectId}/members/{UserId}");
    Summary(s => s.Summary = "Remove project member");
  }
}

public class UpdateProjectMemberRole(IMediator mediator)
  : BaseEndpoint<UpdateProjectMemberRoleCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.ProjectsView);
    Put("/projects/{ProjectId}/members/{UserId}");
    Summary(s => s.Summary = "Update project member role");
  }
}

public sealed class UpdateProjectMemberRoleCommandValidator : Validator<UpdateProjectMemberRoleCommand>
{
  public UpdateProjectMemberRoleCommandValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Role).IsInEnum();
  }
}
