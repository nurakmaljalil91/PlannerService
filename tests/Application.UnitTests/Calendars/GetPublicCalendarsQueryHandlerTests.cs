#nullable enable
using Application.Calendars.Queries.GetPublicCalendars;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Calendars;

/// <summary>
/// Unit tests for <see cref="GetPublicCalendarsQueryHandler"/>.
/// </summary>
public class GetPublicCalendarsQueryHandlerTests
{
    /// <summary>
    /// Verifies that only calendars marked as public are returned.
    /// </summary>
    [Fact]
    public async Task Handle_ReturnsOnlyPublicCalendars()
    {
        await using var context = TestDbContextFactory.Create();

        context.Calendars.Add(new Calendar { Title = "Public Cal", IsPublic = true, UserId = Guid.NewGuid() });
        context.Calendars.Add(new Calendar { Title = "Private Cal", IsPublic = false, UserId = Guid.NewGuid() });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPublicCalendarsQueryHandler(context);
        var result = await handler.Handle(new GetPublicCalendarsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items!);
        Assert.Equal("Public Cal", result.Data.Items!.First().Title);
    }

    /// <summary>
    /// Verifies that an empty result is returned when there are no public calendars.
    /// </summary>
    [Fact]
    public async Task Handle_NoPublicCalendars_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.Create();

        context.Calendars.Add(new Calendar { Title = "Private Cal", IsPublic = false, UserId = Guid.NewGuid() });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPublicCalendarsQueryHandler(context);
        var result = await handler.Handle(new GetPublicCalendarsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.Items!);
    }
}
