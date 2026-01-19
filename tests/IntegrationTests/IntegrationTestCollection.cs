#nullable enable
namespace IntegrationTests;

/// <summary>
/// A collection fixture for integration tests.
/// </summary>
[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<ApiFactory>
{
}
