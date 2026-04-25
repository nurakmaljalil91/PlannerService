using Infrastructure.Data;

namespace Infrastructure.UnitTests.Data;

/// <summary>
/// Unit tests for <see cref="ApplicationDbContextInitialiser"/>.
/// </summary>
public class ApplicationDbContextInitialiserTests
{
    /// <summary>
    /// Ensures the seed method completes without error when the database is empty.
    /// </summary>
    [Fact]
    public async Task TrySeedAsync_CompletesSuccessfully()
    {
        var task = ApplicationDbContextInitialiser.TrySeedAsync();

        await task;

        Assert.True(task.IsCompletedSuccessfully);
    }
}
