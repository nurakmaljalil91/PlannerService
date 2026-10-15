#nullable enable
using Application.Common.Exceptions;
using Application.Reminders.Commands.CreateReminder;
using Application.Reminders.Commands.DeleteReminder;
using Application.Reminders.Commands.UpdateReminder;
using Application.Reminders.Queries.GetReminderById;
using Application.Reminders.Queries.GetReminders;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Reminders;

/// <summary>
/// Ownership tests for the reminder handlers: reminders inherit access from their parent event's calendar.
/// </summary>
public class ReminderOwnershipTests
{
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Caller = Guid.NewGuid();

    /// <summary>
    /// Verifies that an unfiltered list contains the caller's own reminders plus subscribed public ones,
    /// that an event filter allows any readable event, and that other users' private reminders never appear.
    /// </summary>
    [Fact]
    public async Task GetReminders_ReturnsOwnAndSubscribedReminders()
    {
        await using var context = TestDbContextFactory.Create();
        var (own, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new GetRemindersQueryHandler(context, new TestUser(Caller));

        var unsubscribed = await handler.Handle(new GetRemindersQuery { Total = 10 }, CancellationToken.None);
        var preview = await handler.Handle(
            new GetRemindersQuery { EventId = otherPublic.EventId, Total = 10 },
            CancellationToken.None);
        var privateFiltered = await handler.Handle(
            new GetRemindersQuery { EventId = otherPrivate.EventId, Total = 10 },
            CancellationToken.None);

        Assert.Equal(own.Title, Assert.Single(unsubscribed.Data!.Items!).Title);
        Assert.Equal(otherPublic.Title, Assert.Single(preview.Data!.Items!).Title);
        Assert.Equal(0, privateFiltered.Data!.TotalCount);

        var publicCalendarId = context.Events.Single(e => e.Id == otherPublic.EventId).CalendarId;
        await PlannerSeed.SubscribeAsync(context, Caller, publicCalendarId);
        var subscribed = await handler.Handle(new GetRemindersQuery { Total = 10 }, CancellationToken.None);

        Assert.Equal(2, subscribed.Data!.TotalCount);
        Assert.DoesNotContain(subscribed.Data.Items!, r => r.Title == otherPrivate.Title);
    }

    /// <summary>
    /// Verifies single-reminder reads: public reminders are returned, other users' private reminders are not found.
    /// </summary>
    [Fact]
    public async Task GetReminderById_AppliesReadRules()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new GetReminderByIdQueryHandler(context, new TestUser(Caller));

        Assert.True((await handler.Handle(new GetReminderByIdQuery { Id = otherPublic.Id }, CancellationToken.None)).Success);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetReminderByIdQuery { Id = otherPrivate.Id }, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a reminder cannot be attached to another user's event and nothing is created.
    /// </summary>
    [Fact]
    public async Task CreateReminder_OnNonOwnedEvent_IsRejected()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new CreateReminderCommandHandler(context, new TestUser(Caller));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(NewReminder(otherPublic.EventId), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(NewReminder(otherPrivate.EventId), CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(NewReminder(999), CancellationToken.None));

        Assert.Equal(3, context.Reminders.Count());
    }

    /// <summary>
    /// Verifies that non-owners cannot update or delete reminders and the reminders stay unchanged.
    /// </summary>
    [Fact]
    public async Task UpdateAndDelete_ByNonOwner_AreRejected()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var user = new TestUser(Caller);
        var update = new UpdateReminderCommandHandler(context, user);
        var delete = new DeleteReminderCommandHandler(context, user);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            update.Handle(new UpdateReminderCommand { Id = otherPublic.Id, Title = "Hacked" }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            update.Handle(new UpdateReminderCommand { Id = otherPrivate.Id, Title = "Hacked" }, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            delete.Handle(new DeleteReminderCommand { Id = otherPublic.Id }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            delete.Handle(new DeleteReminderCommand { Id = otherPrivate.Id }, CancellationToken.None));

        Assert.Equal(3, context.Reminders.Count());
        Assert.DoesNotContain(context.Reminders, r => r.Title == "Hacked");
    }

    /// <summary>
    /// Verifies that the owner can update their own reminder.
    /// </summary>
    [Fact]
    public async Task UpdateReminder_ByOwner_Succeeds()
    {
        await using var context = TestDbContextFactory.Create();
        var (own, _, _) = await SeedAsync(context);

        var result = await new UpdateReminderCommandHandler(context, new TestUser(Caller))
            .Handle(new UpdateReminderCommand { Id = own.Id, Title = "Renamed" }, CancellationToken.None);

        Assert.Equal("Renamed", result.Data!.Title);
    }

    /// <summary>
    /// Seeds one reminder on an event in each of: the caller's private calendar, another user's public calendar,
    /// and that user's private calendar.
    /// </summary>
    private static async Task<(Reminder Own, Reminder OtherPublic, Reminder OtherPrivate)> SeedAsync(TestApplicationDbContext context)
    {
        var own = await PlannerSeed.ReminderAsync(context,
            await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, Caller)), "Caller's");
        var otherPublic = await PlannerSeed.ReminderAsync(context,
            await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, Owner, isPublic: true)), "Public");
        var otherPrivate = await PlannerSeed.ReminderAsync(context,
            await PlannerSeed.EventAsync(context, await PlannerSeed.CalendarAsync(context, Owner)), "Owner's private");
        return (own, otherPublic, otherPrivate);
    }

    private static CreateReminderCommand NewReminder(long eventId)
        => new() { Title = "New", ReminderDateTime = new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc), EventId = eventId };
}
