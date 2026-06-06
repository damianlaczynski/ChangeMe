using ChangeMe.Backend.Domain.Aggregates.Project.Entities;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.Domain.Aggregates.Project;

public class Project : Entity, IAggregateRoot
{
  private readonly List<ProjectMember> members = [];
  private readonly List<ProjectMembershipHistoryEntry> membershipHistory = [];
  private readonly List<ProjectOperationHistoryEntry> operationHistory = [];

  private Project() { }

  public string Name { get; private set; } = string.Empty;
  public string NormalizedName { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public bool IsSystem { get; private set; }

  public IReadOnlyCollection<ProjectMember> Members => members.AsReadOnly();
  public IReadOnlyCollection<ProjectMembershipHistoryEntry> MembershipHistory => membershipHistory.AsReadOnly();
  public IReadOnlyCollection<ProjectOperationHistoryEntry> OperationHistory => operationHistory.AsReadOnly();

  public static Result<Project> CreateDefault(Guid actorUserId)
  {
    var project = new Project
    {
      Name = ProjectConstraints.DefaultProjectName,
      NormalizedName = NormalizeName(ProjectConstraints.DefaultProjectName),
      Description = string.Empty,
      IsSystem = true,
      CreatedBy = actorUserId,
      UpdatedBy = actorUserId,
    };

    var operationResult = project.AddOperationHistoryEntry(
      ProjectOperationHistoryEventType.PROJECT_CREATED,
      actorUserId,
      "Project created.");
    if (!operationResult.IsSuccess)
      return operationResult.Map();

    return Result.Success(project);
  }

  public static Result<Project> Create(string name, string? description, Guid creatorUserId)
  {
    var validationErrors = ValidateName(name);
    ValidateDescription(description, validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var normalizedName = NormalizeName(name);
    var project = new Project
    {
      Name = name.Trim(),
      NormalizedName = normalizedName,
      Description = description?.Trim() ?? string.Empty,
      IsSystem = false,
    };

    var operationResult = project.AddOperationHistoryEntry(
      ProjectOperationHistoryEventType.PROJECT_CREATED,
      creatorUserId,
      "Project created.");
    if (!operationResult.IsSuccess)
      return operationResult.Map();

    var memberResult = project.AddMemberInternal(creatorUserId, ProjectRole.OWNER, creatorUserId, recordHistory: true);
    if (!memberResult.IsSuccess)
      return memberResult.Map();

    return Result.Success(project);
  }

  public Result<Project> Update(string name, string? description, Guid actorUserId)
  {
    if (IsSystem)
      return Result.Forbidden("System projects cannot be modified.");

    var validationErrors = ValidateName(name);
    ValidateDescription(description, validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var normalizedName = NormalizeName(name);
    var trimmedName = name.Trim();
    var trimmedDescription = description?.Trim() ?? string.Empty;

    if (!string.Equals(Name, trimmedName, StringComparison.Ordinal))
    {
      var previousName = Name;
      Name = trimmedName;
      NormalizedName = normalizedName;

      var nameHistoryResult = AddOperationHistoryEntry(
        ProjectOperationHistoryEventType.NAME_CHANGED,
        actorUserId,
        "Name changed.",
        previousName,
        trimmedName);
      if (!nameHistoryResult.IsSuccess)
        return nameHistoryResult.Map();
    }

    if (!string.Equals(Description, trimmedDescription, StringComparison.Ordinal))
    {
      Description = trimmedDescription;

      var descriptionHistoryResult = AddOperationHistoryEntry(
        ProjectOperationHistoryEventType.DESCRIPTION_CHANGED,
        actorUserId,
        "Description changed.");
      if (!descriptionHistoryResult.IsSuccess)
        return descriptionHistoryResult.Map();
    }

    return Result.Success(this);
  }

  public Result<ProjectMember> AddMember(Guid userId, ProjectRole role, Guid actorUserId)
  {
    if (members.Any(m => m.UserId == userId))
      return Result.Conflict("User is already a member of this project.");

    return AddMemberInternal(userId, role, actorUserId, recordHistory: true);
  }

  public Result EnsureMember(Guid userId, ProjectRole role, Guid actorUserId, bool useSystemActor = false)
  {
    if (members.Any(m => m.UserId == userId))
      return Result.Success();

    var effectiveActorId = useSystemActor ? ProjectAuthorization.SystemActorUserId : actorUserId;
    return AddMemberInternal(userId, role, effectiveActorId, recordHistory: true).Map();
  }

  public Result RemoveMember(Guid userId, Guid actorUserId)
  {
    var member = members.FirstOrDefault(m => m.UserId == userId);
    if (member is null)
      return Result.NotFound();

    if (member.Role == ProjectRole.OWNER && members.Count(m => m.Role == ProjectRole.OWNER) == 1)
    {
      if (userId == actorUserId)
        return Result.Conflict("Assign another owner before removing yourself.");

      return Result.Conflict("Project must have at least one owner.");
    }

    var previousRole = FormatRole(member.Role);
    members.Remove(member);

    var historyResult = AddMembershipHistoryEntry(
      ProjectMembershipHistoryEventType.MEMBER_REMOVED,
      actorUserId,
      userId,
      "Member removed.",
      previousRole,
      currentValue: null);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    return Result.Success();
  }

  public Result ChangeMemberRole(Guid userId, ProjectRole newRole, Guid actorUserId)
  {
    var member = members.FirstOrDefault(m => m.UserId == userId);
    if (member is null)
      return Result.NotFound();

    if (member.Role == newRole)
      return Result.Conflict("Member already has this role.");

    if (member.Role == ProjectRole.OWNER
        && newRole != ProjectRole.OWNER
        && members.Count(m => m.Role == ProjectRole.OWNER) == 1)
    {
      if (userId == actorUserId)
        return Result.Conflict("Assign another owner before changing your own role.");

      return Result.Conflict("Project must have at least one owner.");
    }

    var previousRole = FormatRole(member.Role);
    var changeResult = member.ChangeRole(newRole);
    if (!changeResult.IsSuccess)
      return changeResult.Map();

    var historyResult = AddMembershipHistoryEntry(
      ProjectMembershipHistoryEventType.MEMBER_ROLE_CHANGED,
      actorUserId,
      userId,
      "Member role changed.",
      previousRole,
      FormatRole(newRole));
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    return Result.Success();
  }

  public ProjectRole? GetMemberRole(Guid userId) =>
    members.FirstOrDefault(m => m.UserId == userId)?.Role;

  public bool HasMember(Guid userId) =>
    members.Any(m => m.UserId == userId);

  public static string NormalizeName(string name) =>
    name.Trim().ToUpperInvariant();

  private Result<ProjectMember> AddMemberInternal(
    Guid userId,
    ProjectRole role,
    Guid actorUserId,
    bool recordHistory)
  {
    var memberResult = ProjectMember.Create(Id, userId, role);
    if (!memberResult.IsSuccess)
      return memberResult.Map();

    members.Add(memberResult.Value);

    if (recordHistory)
    {
      var historyResult = AddMembershipHistoryEntry(
        ProjectMembershipHistoryEventType.MEMBER_ADDED,
        actorUserId,
        userId,
        "Member added.",
        previousValue: null,
        FormatRole(role));
      if (!historyResult.IsSuccess)
        return historyResult.Map();
    }

    return Result.Success(memberResult.Value);
  }

  private Result AddMembershipHistoryEntry(
    ProjectMembershipHistoryEventType eventType,
    Guid actorUserId,
    Guid affectedUserId,
    string summary,
    string? previousValue,
    string? currentValue)
  {
    var entryResult = ProjectMembershipHistoryEntry.Create(
      Id,
      eventType,
      actorUserId,
      affectedUserId,
      summary,
      previousValue,
      currentValue);

    if (!entryResult.IsSuccess)
      return entryResult.Map();

    membershipHistory.Add(entryResult.Value);
    return Result.Success();
  }

  private Result AddOperationHistoryEntry(
    ProjectOperationHistoryEventType eventType,
    Guid actorUserId,
    string summary,
    string? previousValue = null,
    string? currentValue = null)
  {
    var entryResult = ProjectOperationHistoryEntry.Create(
      Id,
      eventType,
      actorUserId,
      summary,
      previousValue,
      currentValue);

    if (!entryResult.IsSuccess)
      return entryResult.Map();

    operationHistory.Add(entryResult.Value);
    return Result.Success();
  }

  private static List<ValidationError> ValidateName(string name)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(name))
      validationErrors.Add(new ValidationError(nameof(Name), "is required"));
    else
    {
      var trimmed = name.Trim();
      if (trimmed.Length < ProjectConstraints.NAME_MIN_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Name), $"must be at least {ProjectConstraints.NAME_MIN_LENGTH} characters"));
      else if (trimmed.Length > ProjectConstraints.NAME_MAX_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Name), $"cannot be longer than {ProjectConstraints.NAME_MAX_LENGTH} characters"));
    }

    return validationErrors;
  }

  private static void ValidateDescription(string? description, List<ValidationError> validationErrors)
  {
    if (description is not null && description.Trim().Length > ProjectConstraints.DESCRIPTION_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {ProjectConstraints.DESCRIPTION_MAX_LENGTH} characters"));
  }

  private static string FormatRole(ProjectRole role) =>
    role switch
    {
      ProjectRole.OWNER => "Owner",
      ProjectRole.MEMBER => "Member",
      ProjectRole.VIEWER => "Viewer",
      _ => role.ToString(),
    };
}

public static class ProjectConstraints
{
  public const string DefaultProjectName = "Default";
  public const int NAME_MIN_LENGTH = 2;
  public const int NAME_MAX_LENGTH = 100;
  public const int DESCRIPTION_MAX_LENGTH = 500;
  public const string DuplicateNameMessage = "A project with this name already exists.";
  public const string HasIssuesDeleteMessage = "Project has one or more issues. Move or delete all issues before deleting this project.";
}
