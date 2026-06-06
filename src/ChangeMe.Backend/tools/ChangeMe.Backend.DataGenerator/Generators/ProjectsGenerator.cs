using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using DomainProject = global::ChangeMe.Backend.Domain.Aggregates.Project.Project;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;
using ProjectConstraints = global::ChangeMe.Backend.Domain.Aggregates.Project.ProjectConstraints;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed record DemoProject(Guid Id, string Name);

internal sealed class ProjectsGenerator(
  ApplicationDbContext dbContext,
  IOptions<DataGeneratorOptions> options)
{
  public async Task<IReadOnlyList<DemoProject>> GenerateAsync(
    IReadOnlyList<DomainUser> demoUsers,
    CancellationToken cancellationToken)
  {
    if (demoUsers.Count == 0)
      return [];

    var config = options.Value;
    if (config.Projects <= 0)
      return [];

    var faker = new Faker { Random = new Randomizer(config.Seed + 2) };
    var projects = new List<DemoProject>();
    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    for (var index = 0; index < config.Projects; index++)
    {
      var owner = PickRandom(demoUsers, faker);
      var name = BuildUniqueProjectName(faker, index, usedNames);
      var description = Truncate(faker.Lorem.Paragraph(), ProjectConstraints.DESCRIPTION_MAX_LENGTH);

      var createResult = DomainProject.Create(name, description, owner.Id);
      if (!createResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create demo project: {string.Join(", ", createResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var project = createResult.Value;
      EntityAudit.Apply(project, owner.Id);
      EntityAudit.Apply(project.Members, owner.Id);
      EntityAudit.Apply(project.MembershipHistory, owner.Id);
      EntityAudit.Apply(project.OperationHistory, owner.Id);

      foreach (var member in demoUsers.Where(u => u.Id != owner.Id))
      {
        var memberResult = project.EnsureMember(member.Id, ProjectRole.MEMBER, owner.Id);
        if (!memberResult.IsSuccess)
          throw new InvalidOperationException($"Failed to add demo member to project: {string.Join(", ", memberResult.ValidationErrors.Select(e => e.ErrorMessage))}");
      }

      EntityAudit.Apply(project.Members.Where(m => m.UserId != owner.Id), owner.Id);
      EntityAudit.Apply(
        project.MembershipHistory.Where(h => h.AffectedUserId != owner.Id),
        owner.Id);

      await dbContext.Projects.AddAsync(project, cancellationToken);
      await dbContext.ProjectMembers.AddRangeAsync(project.Members, cancellationToken);
      await dbContext.ProjectMembershipHistory.AddRangeAsync(project.MembershipHistory, cancellationToken);
      await dbContext.ProjectOperationHistory.AddRangeAsync(project.OperationHistory, cancellationToken);

      projects.Add(new DemoProject(project.Id, project.Name));
    }

    if (projects.Count > 0)
      await dbContext.SaveChangesAsync(cancellationToken);

    return projects;
  }

  private static string BuildUniqueProjectName(Faker faker, int index, HashSet<string> usedNames)
  {
    for (var attempt = 0; attempt < 20; attempt++)
    {
      var candidate = Truncate(faker.Company.CompanyName(), ProjectConstraints.NAME_MAX_LENGTH);
      if (candidate.Length < ProjectConstraints.NAME_MIN_LENGTH)
        candidate = $"Demo project {index + 1}";

      if (usedNames.Add(candidate))
        return candidate;
    }

    var fallback = $"Demo project {index + 1}";
    usedNames.Add(fallback);
    return fallback;
  }

  private static T PickRandom<T>(IReadOnlyList<T> items, Faker faker) =>
    items[faker.Random.Int(0, items.Count - 1)];

  private static string Truncate(string value, int maxLength)
  {
    var trimmed = value.Trim();
    return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
  }
}
