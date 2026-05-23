namespace ChangeMe.Backend.IntegrationTests.Fixtures;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PublicRegistrationIntegrationTestCollection : ICollectionFixture<PublicRegistrationDisabledWebApplicationFactory>
{
  public const string Name = "Public registration integration tests";
}
