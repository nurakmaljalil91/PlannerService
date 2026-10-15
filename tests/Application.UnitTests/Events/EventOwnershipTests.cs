#nullable enable
using Application.Common.Exceptions;
using Application.Events.Commands.CreateEvent;
using Application.Events.Commands.DeleteEvent;
using Application.Events.Commands.UpdateEvent;
using Application.Events.Queries.GetEventById;
using Application.Events.Queries.GetEvents;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Events;

/// <summary>
/// Ownership tests for the event handlers: events are readable from owned or public calendars
/// and writable only by the calendar owner.
/// </summary>
public class EventOwnershipTests
{
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Stranger = Guid.NewGuid();

    /// <summary>
    /// Verifies that an unfiltered list returns the caller's own events plus subscribed public events, with a matching total.
    /// </summary>
    [Fact]
    public async Task GetEvents_Unfiltered_ReturnsOwnAndSubscribedEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var (mine, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new GetEventsQueryHandler(context, new TestUser(Stranger));

        var unsubscribed = await handler.Handle(new GetEventsQuery { Total = 10 }, CancellationToken.None);

        Assert.Equal(1, unsubscribed.Data!.TotalCount);
        Assert.Equal(mine.Title, Assert.Single(unsubscribed.Data.Items!).Title);

        await PlannerSeed.SubscribeAsync(context, Stranger, otherPublic.CalendarId);
        var subscribed = await handler.Handle(new GetEventsQuery { Total = 10 }, CancellationToken.None);

        Assert.Equal(2, subscribed.Data!.TotalCount);
        Assert.Contains(subscribed.Data.Items!, e => e.Title == otherPublic.Title);
        Assert.DoesNotContain(subscribed.Data.Items!, e => e.Title == otherPrivate.Title);

        var ownerResult = await new GetEventsQueryHandler(context, new TestUser(Owner))
            .Handle(new GetEventsQuery { Total = 10 }, CancellationToken.None);

        Assert.Equal(2, ownerResult.Data!.TotalCount);
        Assert.DoesNotContain(ownerResult.Data.Items!, e => e.Title == mine.Title);
    }

    /// <summary>
    /// Verifies that an unsubscribed public calendar can be previewed by filtering on it.
    /// </summary>
    [Fact]
    public async Task GetEvents_FilteredByUnsubscribedPublicCalendar_ReturnsItsEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, _) = await SeedAsync(context);

        var result = await new GetEventsQueryHandler(context, new TestUser(Stranger))
            .Handle(new GetEventsQuery { CalendarId = otherPublic.CalendarId, Total = 10 }, CancellationToken.None);

        Assert.Equal(otherPublic.Title, Assert.Single(result.Data!.Items!).Title);
    }

    /// <summary>
    /// Verifies that a subscription stops exposing events once the calendar is no longer public.
    /// </summary>
    [Fact]
    public async Task GetEvents_SubscriptionToCalendarNoLongerPublic_ExcludesItsEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, _) = await SeedAsync(context);
        await PlannerSeed.SubscribeAsync(context, Stranger, otherPublic.CalendarId);
        var calendar = await context.Calendars.FindAsync(otherPublic.CalendarId);
        calendar!.IsPublic = false;
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await new GetEventsQueryHandler(context, new TestUser(Stranger))
            .Handle(new GetEventsQuery { Total = 10 }, CancellationToken.None);

        Assert.DoesNotContain(result.Data!.Items!, e => e.Title == otherPublic.Title);
    }

    /// <summary>
    /// Verifies that filtering by another user's private calendar returns nothing, including with a range filter.
    /// </summary>
    [Fact]
    public async Task GetEvents_FilteredByOtherUsersPrivateCalendar_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, _, otherPrivate) = await SeedAsync(context);
        var handler = new GetEventsQueryHandler(context, new TestUser(Stranger));

        var filtered = await handler.Handle(
            new GetEventsQuery { CalendarId = otherPrivate.CalendarId, Total = 10 },
            CancellationToken.None);
        var ranged = await handler.Handle(
            new GetEventsQuery
            {
                CalendarId = otherPrivate.CalendarId,
                RangeStart = otherPrivate.StartTime.AddDays(-1),
                RangeEnd = otherPrivate.StartTime.AddDays(1),
                Total = 10
            },
            CancellationToken.None);

        Assert.Equal(0, filtered.Data!.TotalCount);
        Assert.Equal(0, ranged.Data!.TotalCount);
    }

    /// <summary>
    /// Verifies single-event reads: own and public events are returned, other users' private events are not found.
    /// </summary>
    [Fact]
    public async Task GetEventById_AppliesReadRules()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var stranger = new GetEventByIdQueryHandler(context, new TestUser(Stranger));
        var owner = new GetEventByIdQueryHandler(context, new TestUser(Owner));

        Assert.True((await stranger.Handle(new GetEventByIdQuery { Id = otherPublic.Id }, CancellationToken.None)).Success);
        Assert.True((await owner.Handle(new GetEventByIdQuery { Id = otherPrivate.Id }, CancellationToken.None)).Success);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            stranger.Handle(new GetEventByIdQuery { Id = otherPrivate.Id }, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that creating an event in a non-owned public calendar is forbidden and creates nothing.
    /// </summary>
    [Fact]
    public async Task CreateEvent_InPublicCalendarNotOwned_ThrowsForbidden()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, _) = await SeedAsync(context);
        var handler = new CreateEventCommandHandler(context, new TestUser(Stranger));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(NewEvent(otherPublic.CalendarId), CancellationToken.None));
        Assert.Equal(3, context.Events.Count());
    }

    /// <summary>
    /// Verifies that creating an event in another user's private calendar, or a missing calendar, is not found.
    /// </summary>
    [Fact]
    public async Task CreateEvent_InPrivateCalendarNotOwnedOrMissing_ThrowsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, _, otherPrivate) = await SeedAsync(context);
        var handler = new CreateEventCommandHandler(context, new TestUser(Stranger));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(NewEvent(otherPrivate.CalendarId), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(NewEvent(999), CancellationToken.None));
        Assert.Equal(3, context.Events.Count());
    }

    /// <summary>
    /// Verifies that the owner of a public calendar can create events in it.
    /// </summary>
    [Fact]
    public async Task CreateEvent_InOwnedPublicCalendar_Succeeds()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, _) = await SeedAsync(context);

        var result = await new CreateEventCommandHandler(context, new TestUser(Owner))
            .Handle(NewEvent(otherPublic.CalendarId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(4, context.Events.Count());
    }

    /// <summary>
    /// Verifies that non-owners cannot update events: forbidden for public calendars, not found for private ones.
    /// </summary>
    [Fact]
    public async Task UpdateEvent_ByNonOwner_IsRejectedAndLeavesEventUnchanged()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new UpdateEventCommandHandler(context, new TestUser(Stranger));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpdateEventCommand { Id = otherPublic.Id, Title = "Hacked" }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateEventCommand { Id = otherPrivate.Id, Title = "Hacked" }, CancellationToken.None));

        Assert.DoesNotContain(context.Events, e => e.Title == "Hacked");
    }

    /// <summary>
    /// Verifies that the owner can update their event.
    /// </summary>
    [Fact]
    public async Task UpdateEvent_ByOwner_Succeeds()
    {
        await using var context = TestDbContextFactory.Create();
        var (mine, _, _) = await SeedAsync(context);

        var result = await new UpdateEventCommandHandler(context, new TestUser(Stranger))
            .Handle(new UpdateEventCommand { Id = mine.Id, Title = "Renamed" }, CancellationToken.None);

        Assert.Equal("Renamed", result.Data!.Title);
    }

    /// <summary>
    /// Verifies that non-owners cannot delete events: forbidden for public calendars, not found for private ones.
    /// </summary>
    [Fact]
    public async Task DeleteEvent_ByNonOwner_IsRejectedAndKeepsEvent()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new DeleteEventCommandHandler(context, new TestUser(Stranger));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new DeleteEventCommand { Id = otherPublic.Id }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeleteEventCommand { Id = otherPrivate.Id }, CancellationToken.None));

        Assert.Equal(3, context.Events.Count());
    }

    /// <summary>
    /// Seeds one event in each of: the stranger's private calendar, the owner's public calendar, and the owner's private calendar.
    /// </summary>
    private static async Task<(Event Mine, Event OtherPublic, Event OtherPrivate)> SeedAsync(TestApplicationDbContext context)
    {
        var mine = await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, Stranger), "Stranger's");
        var otherPublic = await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, Owner, isPublic: true), "Public");
        var otherPrivate = await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, Owner), "Owner's private");
        return (mine, otherPublic, otherPrivate);
    }

    private static CreateEventCommand NewEvent(long calendarId)
    {
        var start = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc);
        return new CreateEventCommand { Title = "Merdeka", StartTime = start, EndTime = start.AddDays(1), IsAllDay = true, CalendarId = calendarId };
    }
}
