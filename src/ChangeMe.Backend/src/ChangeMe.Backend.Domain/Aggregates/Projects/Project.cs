using ChangeMe.Backend.Domain.Aggregates.Projects.Entities;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Projects;

public class Project : Entity, IAggregateRoot
{
  private readonly List<ProjectMember> members = [];

  private Project() { }

  public string Name { get; private set; } = string.Empty;
  public string Key { get; private set; } = string.Empty;
  public string? Description { get; private set; }
  public ProjectStatus Status { get; private set; } = ProjectStatus.ACTIVE;
  public ProjectVisibility Visibility { get; private set; } = ProjectVisibility.INTERNAL;
  public string Color { get; private set; } = ProjectConstraints.DEFAULT_COLOR;

  public IReadOnlyCollection<ProjectMember> Members => members.AsReadOnly();

  public static Result<Project> Create(
    string name,
    string key,
    string? description,
    ProjectVisibility visibility,
    string? color = null)
  {
    var validationErrors = ValidateProfile(name, key, description, visibility, color);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var project = new Project
    {
      Name = name.Trim(),
      Key = NormalizeKey(key),
      Description = NormalizeDescription(description),
      Visibility = visibility,
      Color = NormalizeColor(color),
      Status = ProjectStatus.ACTIVE
    };

    return Result.Success(project);
  }

  public Result UpdateProfile(
    string name,
    string key,
    string? description,
    ProjectVisibility visibility,
    ProjectStatus status,
    string? color = null)
  {
    var validationErrors = ValidateProfile(name, key, description, visibility, color);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    if (!Enum.IsDefined(status))
      return Result.Invalid([new ValidationError(nameof(Status), "invalid status")]);

    Name = name.Trim();
    Key = NormalizeKey(key);
    Description = NormalizeDescription(description);
    Visibility = visibility;
    Status = status;
    Color = NormalizeColor(color);

    return Result.Success();
  }

  public Result<ProjectMember> AddMember(Guid userId, ProjectMemberRole role)
  {
    if (members.Any(m => m.UserId == userId))
      return Result.Conflict(ProjectMemberMessages.UserAlreadyMember);

    var memberResult = ProjectMember.Create(Id, userId, role);
    if (!memberResult.IsSuccess)
      return memberResult.Map();

    members.Add(memberResult.Value);
    return memberResult;
  }

  public Result<ProjectMember> UpdateMemberRole(Guid userId, ProjectMemberRole role)
  {
    var member = members.FirstOrDefault(m => m.UserId == userId);
    if (member is null)
      return Result.NotFound();

    if (member.Role == ProjectMemberRole.OWNER
        && role != ProjectMemberRole.OWNER
        && members.Count(m => m.Role == ProjectMemberRole.OWNER) <= 1)
      return Result.Error(ProjectMemberMessages.CannotRemoveLastOwner);

    return member.UpdateRole(role).Map(() => member);
  }

  public Result<ProjectMember> RemoveMember(Guid userId)
  {
    var member = members.FirstOrDefault(m => m.UserId == userId);
    if (member is null)
      return Result.NotFound();

    if (member.Role == ProjectMemberRole.OWNER && members.Count(m => m.Role == ProjectMemberRole.OWNER) <= 1)
      return Result.Error(ProjectMemberMessages.CannotRemoveLastOwner);

    members.Remove(member);
    return Result.Success(member);
  }

  public bool IsAccessibleBy(Guid userId) =>
    Visibility == ProjectVisibility.INTERNAL || members.Any(m => m.UserId == userId);

  public ProjectMemberRole? GetMemberRole(Guid userId) =>
    members.FirstOrDefault(m => m.UserId == userId)?.Role;

  public bool CanManage(Guid userId) =>
    GetMemberRole(userId) == ProjectMemberRole.OWNER;

  public static string NormalizeKey(string key) => key.Trim().ToUpperInvariant();

  private static string? NormalizeDescription(string? description) =>
    string.IsNullOrWhiteSpace(description) ? null : description.Trim();

  private static string NormalizeColor(string? color)
  {
    if (string.IsNullOrWhiteSpace(color))
      return ProjectConstraints.DEFAULT_COLOR;

    var normalized = color.Trim();
    return ProjectConstraints.IsValidColor(normalized) ? normalized : ProjectConstraints.DEFAULT_COLOR;
  }

  private static List<ValidationError> ValidateProfile(
    string name,
    string key,
    string? description,
    ProjectVisibility visibility,
    string? color)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(name))
      validationErrors.Add(new ValidationError(nameof(Name), "cannot be empty"));
    else if (name.Trim().Length < ProjectConstraints.NAME_MIN_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Name), $"must be at least {ProjectConstraints.NAME_MIN_LENGTH} characters"));
    else if (name.Trim().Length > ProjectConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Name), $"cannot be longer than {ProjectConstraints.NAME_MAX_LENGTH} characters"));

    if (string.IsNullOrWhiteSpace(key))
      validationErrors.Add(new ValidationError(nameof(Key), "cannot be empty"));
    else if (!ProjectConstraints.IsValidKey(key))
      validationErrors.Add(new ValidationError(nameof(Key), $"must be {ProjectConstraints.KEY_MIN_LENGTH}-{ProjectConstraints.KEY_MAX_LENGTH} uppercase letters or digits"));

    if (description is not null && description.Trim().Length > ProjectConstraints.DESCRIPTION_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {ProjectConstraints.DESCRIPTION_MAX_LENGTH} characters"));

    if (!Enum.IsDefined(visibility))
      validationErrors.Add(new ValidationError(nameof(Visibility), "invalid visibility"));

    if (color is not null && !string.IsNullOrWhiteSpace(color) && !ProjectConstraints.IsValidColor(color.Trim()))
      validationErrors.Add(new ValidationError(nameof(Color), "must be a valid hex color (for example #3B82F6)"));

    return validationErrors;
  }
}

public static class ProjectConstraints
{
  public const int NAME_MIN_LENGTH = 2;
  public const int NAME_MAX_LENGTH = 100;
  public const int KEY_MIN_LENGTH = 2;
  public const int KEY_MAX_LENGTH = 10;
  public const int DESCRIPTION_MAX_LENGTH = 1000;
  public const string DEFAULT_COLOR = "#3B82F6";

  public static bool IsValidKey(string key)
  {
    var normalized = key.Trim();
    if (normalized.Length < KEY_MIN_LENGTH || normalized.Length > KEY_MAX_LENGTH)
      return false;

    foreach (var character in normalized)
    {
      if (!char.IsAsciiLetterOrDigit(character))
        return false;
    }

    return true;
  }

  public static bool IsValidColor(string color) =>
    color.Length == 7
    && color[0] == '#'
    && color[1..].All(c => char.IsAsciiHexDigit(c));
}

public static class ProjectMemberMessages
{
  public const string UserAlreadyMember = "User is already a member of this project.";
  public const string CannotRemoveLastOwner = "Cannot remove the last owner from the project.";
}
