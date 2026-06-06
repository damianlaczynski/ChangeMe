using ChangeMe.Backend.Domain.Aggregates.Project.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Project.Entities;

public class ProjectMember : Entity
{
  private ProjectMember() { }

  public Guid ProjectId { get; private set; }
  public Guid UserId { get; private set; }
  public ProjectRole Role { get; private set; }

  public static Result<ProjectMember> Create(Guid projectId, Guid userId, ProjectRole role)
  {
    var validationErrors = new List<ValidationError>();

    if (projectId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(ProjectId), "cannot be empty"));

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (!Enum.IsDefined(role))
      validationErrors.Add(new ValidationError(nameof(Role), "invalid project role"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new ProjectMember
    {
      ProjectId = projectId,
      UserId = userId,
      Role = role,
    });
  }

  internal Result ChangeRole(ProjectRole role)
  {
    if (!Enum.IsDefined(role))
      return Result.Invalid(new ValidationError(nameof(Role), "invalid project role"));

    if (Role == role)
      return Result.Invalid(new ValidationError(nameof(Role), "member already has this role"));

    Role = role;
    return Result.Success();
  }
}
