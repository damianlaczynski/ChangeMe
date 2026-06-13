using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using Microsoft.Extensions.Options;
using DomainProject = global::ChangeMe.Backend.Domain.Aggregates.Projects.Project;
using ProjectConstraints = global::ChangeMe.Backend.Domain.Aggregates.Projects.ProjectConstraints;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class ProjectsGenerator(IOptions<DataGeneratorOptions> options)
{
  public IReadOnlyList<DomainProject> Generate(IReadOnlyList<DomainUser> demoUsers)
  {
    var config = options.Value;
    var faker = new Faker { Random = new Randomizer(config.Seed) };
    var projects = new List<DomainProject>();
    var projectCount = Math.Max(2, config.Projects);

    for (var index = 0; index < projectCount; index++)
    {
      var owner = demoUsers[faker.Random.Int(0, demoUsers.Count - 1)];
      var name = faker.Commerce.Department();
      var key = GenerateKey(name, index, faker);
      var visibility = faker.Random.Bool(0.75f)
        ? ProjectVisibility.INTERNAL
        : ProjectVisibility.PRIVATE;

      var projectResult = DomainProject.Create(
        name,
        key,
        faker.Lorem.Sentence(8),
        visibility,
        faker.Internet.Color());

      if (!projectResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create demo project: {string.Join(", ", projectResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var project = projectResult.Value;
      project.AddMember(owner.Id, ProjectMemberRole.OWNER);

      var memberCount = faker.Random.Int(1, Math.Min(4, demoUsers.Count));
      foreach (var member in faker.PickRandom(demoUsers, memberCount).DistinctBy(u => u.Id))
      {
        if (member.Id == owner.Id)
          continue;

        project.AddMember(member.Id, faker.PickRandom(ProjectMemberRole.MEMBER, ProjectMemberRole.VIEWER));
      }

      EntityAudit.Apply(project, owner.Id);
      EntityAudit.Apply(project.Members, owner.Id);
      projects.Add(project);
    }

    return projects;
  }

  private static string GenerateKey(string name, int index, Faker faker)
  {
    var letters = new string(name.Where(char.IsLetter).Take(4).ToArray()).ToUpperInvariant();
    if (letters.Length < ProjectConstraints.KEY_MIN_LENGTH)
      letters = $"P{index + 1}";

    return letters.Length > ProjectConstraints.KEY_MAX_LENGTH
      ? letters[..ProjectConstraints.KEY_MAX_LENGTH]
      : letters;
  }
}
