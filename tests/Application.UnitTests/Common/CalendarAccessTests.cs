#nullable enable
using Application.Common.Exceptions;
using Application.Common.Security;
using Application.UnitTests.TestInfrastructure;

namespace Application.UnitTests.Common;

/// <summary>
/// Unit tests for <see cref="CalendarAccess"/>.
/// </summary>
public class CalendarAccessTests
{
    /// <summary>
    /// Verifies that a user can read their own calendars and public calendars but not other users' private calendars.
    /// </summary>
    [Fact]
    public async Task ReadableBy_ReturnsOwnedAndPublicCalendars()
    {
        await using var context = TestDbContextFactory.Create();
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var mine = await PlannerSeed.CalendarAsync(context, me);
        var otherPublic = await PlannerSeed.CalendarAsync(context, other, isPublic: true);
        await PlannerSeed.CalendarAsync(context, other);

        var ids = context.Calendars.ReadableBy(me).Select(c => c.Id).OrderBy(id => id).ToList();

        Assert.Equal(new[] { mine.Id, otherPublic.Id }, ids);
    }

    /// <summary>
    /// Verifies that an unidentified user can read only public calendars and owns nothing.
    /// </summary>
    [Fact]
    public async Task NullUser_ReadsOnlyPublicAndOwnsNothing()
    {
        await using var context = TestDbContextFactory.Create();
        var publicCalendar = await PlannerSeed.CalendarAsync(context, Guid.NewGuid(), isPublic: true);
        await PlannerSeed.CalendarAsync(context, null);

        Assert.Equal(new[] { publicCalendar.Id }, context.Calendars.ReadableBy(null).Select(c => c.Id).ToList());
        Assert.Empty(context.Calendars.OwnedBy(null).ToList());
    }

    /// <summary>
    /// Verifies that <see cref="CalendarAccess.OwnedBy"/> excludes public calendars owned by others.
    /// </summary>
    [Fact]
    public async Task OwnedBy_ReturnsOnlyOwnedCalendars()
    {
        await using var context = TestDbContextFactory.Create();
        var me = Guid.NewGuid();
        var mine = await PlannerSeed.CalendarAsync(context, me);
        await PlannerSeed.CalendarAsync(context, Guid.NewGuid(), isPublic: true);

        Assert.Equal(new[] { mine.Id }, context.Calendars.OwnedBy(me).Select(c => c.Id).ToList());
    }

    /// <summary>
    /// Verifies that the owner may write to their calendar.
    /// </summary>
    [Fact]
    public async Task EnsureCalendarWritable_Owner_Succeeds()
    {
        await using var context = TestDbContextFactory.Create();
        var me = Guid.NewGuid();
        var mine = await PlannerSeed.CalendarAsync(context, me);

        var exception = await Record.ExceptionAsync(() =>
            context.EnsureCalendarWritableAsync(mine.Id, me, "missing", CancellationToken.None));

        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that writing to another user's public calendar is forbidden.
    /// </summary>
    [Fact]
    public async Task EnsureCalendarWritable_PublicNotOwned_ThrowsForbidden()
    {
        await using var context = TestDbContextFactory.Create();
        var publicCalendar = await PlannerSeed.CalendarAsync(context, Guid.NewGuid(), isPublic: true);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            context.EnsureCalendarWritableAsync(publicCalendar.Id, Guid.NewGuid(), "missing", CancellationToken.None));
    }

    /// <summary>
    /// Verifies that another user's private calendar, a missing calendar, and an unidentified caller all yield not found
    /// or forbidden without revealing private data.
    /// </summary>
    [Fact]
    public async Task EnsureCalendarWritable_PrivateNotOwnedOrMissing_ThrowsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var otherPrivate = await PlannerSeed.CalendarAsync(context, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.EnsureCalendarWritableAsync(otherPrivate.Id, Guid.NewGuid(), "missing", CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.EnsureCalendarWritableAsync(999, Guid.NewGuid(), "missing", CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.EnsureCalendarWritableAsync(otherPrivate.Id, null, "missing", CancellationToken.None));
    }

    /// <summary>
    /// Verifies that event access follows the event's calendar.
    /// </summary>
    [Fact]
    public async Task EnsureEventAccess_FollowsCalendar()
    {
        await using var context = TestDbContextFactory.Create();
        var owner = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var publicEvent = await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, owner, isPublic: true));
        var privateEvent = await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, owner));

        await context.EnsureEventReadableAsync(publicEvent.Id, stranger, "missing", CancellationToken.None);
        await context.EnsureEventWritableAsync(privateEvent.Id, owner, "missing", CancellationToken.None);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            context.EnsureEventWritableAsync(publicEvent.Id, stranger, "missing", CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.EnsureEventReadableAsync(privateEvent.Id, stranger, "missing", CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            context.EnsureEventReadableAsync(999, owner, "missing", CancellationToken.None));
    }

    /// <summary>
    /// Verifies that unfiltered listing covers owned and subscribed public calendars, and that a filter
    /// allows any readable calendar but never another user's private one.
    /// </summary>
    [Fact]
    public async Task ListedCalendarIds_AppliesDefaultAndFilterRules()
    {
        await using var context = TestDbContextFactory.Create();
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var mine = await PlannerSeed.CalendarAsync(context, me);
        var subscribedPublic = await PlannerSeed.CalendarAsync(context, other, isPublic: true);
        var unsubscribedPublic = await PlannerSeed.CalendarAsync(context, other, isPublic: true);
        var otherPrivate = await PlannerSeed.CalendarAsync(context, other);
        await PlannerSeed.SubscribeAsync(context, me, subscribedPublic.Id);
        await PlannerSeed.SubscribeAsync(context, me, otherPrivate.Id);

        Assert.Equal(
            new[] { mine.Id, subscribedPublic.Id },
            context.ListedCalendarIds(me, null).OrderBy(id => id).ToList());
        Assert.Equal(new[] { unsubscribedPublic.Id }, context.ListedCalendarIds(me, unsubscribedPublic.Id).ToList());
        Assert.Empty(context.ListedCalendarIds(me, otherPrivate.Id).ToList());
        Assert.Empty(context.ListedCalendarIds(null, null).ToList());
    }
}
