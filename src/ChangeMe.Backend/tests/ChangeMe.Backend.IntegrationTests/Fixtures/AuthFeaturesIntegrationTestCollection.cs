namespace ChangeMe.Backend.IntegrationTests.Fixtures;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AuthFeaturesIntegrationTestCollection : ICollectionFixture<AuthFeaturesWebApplicationFactory>
{
  public const string Name = "Auth features integration tests";
}
