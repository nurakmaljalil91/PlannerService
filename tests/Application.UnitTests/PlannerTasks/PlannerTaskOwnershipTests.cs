#nullable enable
using Application.Common.Exceptions;
using Application.PlannerTasks.Commands.CompletePlannerTask;
using Application.PlannerTasks.Commands.CreatePlannerTask;
using Application.PlannerTasks.Commands.DeletePlannerTask;
using Application.PlannerTasks.Commands.UpdatePlannerTask;
using Application.PlannerTasks.Queries.GetPlannerTasks;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.PlannerTasks;

/// <summary>
/// Ownership tests for the planner task handlers: tasks are readable from owned or public calendars
/// and writable only by the calendar owner.
/// </summary>
public class PlannerTaskOwnershipTests
{
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Caller = Guid.NewGuid();

    /// <summary>
    /// Verifies that an unfiltered list contains the caller's own tasks plus subscribed public tasks, with a matching total.
    /// </summary>
    [Fact]
    public async Task GetPlannerTasks_Unfiltered_ReturnsOwnAndSubscribedTasks()
    {
        await using var context = TestDbContextFactory.Create();
        var (own, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new GetPlannerTasksQueryHandler(context, new TestUser(Caller));

        var unsubscribed = await handler.Handle(new GetPlannerTasksQuery { Total = 10 }, CancellationToken.None);

        Assert.Equal(own.Title, Assert.Single(unsubscribed.Data!.Items!).Title);

        await PlannerSeed.SubscribeAsync(context, Caller, otherPublic.CalendarId);
        var subscribed = await handler.Handle(new GetPlannerTasksQuery { Total = 10 }, CancellationToken.None);
        var preview = await handler.Handle(
            new GetPlannerTasksQuery { CalendarId = otherPublic.CalendarId, Total = 10 },
            CancellationToken.None);

        Assert.Equal(2, subscribed.Data!.TotalCount);
        Assert.Contains(subscribed.Data.Items!, t => t.Title == otherPublic.Title);
        Assert.DoesNotContain(subscribed.Data.Items!, t => t.Title == otherPrivate.Title);
        Assert.Equal(otherPublic.Title, Assert.Single(preview.Data!.Items!).Title);
    }

    /// <summary>
    /// Verifies that filtering by another user's private calendar returns nothing.
    /// </summary>
    [Fact]
    public async Task GetPlannerTasks_FilteredByOtherUsersPrivateCalendar_ReturnsEmpty()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, _, otherPrivate) = await SeedAsync(context);

        var result = await new GetPlannerTasksQueryHandler(context, new TestUser(Caller))
            .Handle(new GetPlannerTasksQuery { CalendarId = otherPrivate.CalendarId, Total = 10 }, CancellationToken.None);

        Assert.Equal(0, result.Data!.TotalCount);
    }

    /// <summary>
    /// Verifies that creating a task in a calendar the caller does not own is rejected and creates nothing.
    /// </summary>
    [Fact]
    public async Task CreatePlannerTask_InNonOwnedCalendar_IsRejected()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var handler = new CreatePlannerTaskCommandHandler(context, new TestUser(Caller));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreatePlannerTaskCommand { Title = "New", CalendarId = otherPublic.CalendarId }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreatePlannerTaskCommand { Title = "New", CalendarId = otherPrivate.CalendarId }, CancellationToken.None));

        Assert.Equal(3, context.PlannerTasks.Count());
    }

    /// <summary>
    /// Verifies that non-owners cannot update, complete or delete tasks and the tasks stay unchanged.
    /// </summary>
    [Fact]
    public async Task UpdateCompleteDelete_ByNonOwner_AreRejected()
    {
        await using var context = TestDbContextFactory.Create();
        var (_, otherPublic, otherPrivate) = await SeedAsync(context);
        var user = new TestUser(Caller);
        var update = new UpdatePlannerTaskCommandHandler(context, user);
        var complete = new CompletePlannerTaskCommandHandler(context, user);
        var delete = new DeletePlannerTaskCommandHandler(context, user);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            update.Handle(new UpdatePlannerTaskCommand { Id = otherPublic.Id, Title = "Hacked" }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            update.Handle(new UpdatePlannerTaskCommand { Id = otherPrivate.Id, Title = "Hacked" }, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            complete.Handle(new CompletePlannerTaskCommand { Id = otherPublic.Id, IsCompleted = true }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            complete.Handle(new CompletePlannerTaskCommand { Id = otherPrivate.Id, IsCompleted = true }, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            delete.Handle(new DeletePlannerTaskCommand { Id = otherPublic.Id }, CancellationToken.None));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            delete.Handle(new DeletePlannerTaskCommand { Id = otherPrivate.Id }, CancellationToken.None));

        Assert.Equal(3, context.PlannerTasks.Count());
        Assert.DoesNotContain(context.PlannerTasks, t => t.Title == "Hacked" || t.IsCompleted);
    }

    /// <summary>
    /// Verifies that the owner can update and complete their own task.
    /// </summary>
    [Fact]
    public async Task UpdateAndComplete_ByOwner_Succeed()
    {
        await using var context = TestDbContextFactory.Create();
        var (own, _, _) = await SeedAsync(context);
        var user = new TestUser(Caller);

        var updated = await new UpdatePlannerTaskCommandHandler(context, user)
            .Handle(new UpdatePlannerTaskCommand { Id = own.Id, Title = "Renamed" }, CancellationToken.None);
        var completed = await new CompletePlannerTaskCommandHandler(context, user)
            .Handle(new CompletePlannerTaskCommand { Id = own.Id, IsCompleted = true }, CancellationToken.None);

        Assert.Equal("Renamed", updated.Data!.Title);
        Assert.True(completed.Data!.IsCompleted);
    }

    /// <summary>
    /// Seeds one task in each of: the caller's private calendar, another user's public calendar, and that user's private calendar.
    /// </summary>
    private static async Task<(PlannerTask Own, PlannerTask OtherPublic, PlannerTask OtherPrivate)> SeedAsync(TestApplicationDbContext context)
    {
        var own = await PlannerSeed.TaskAsync(context, await PlannerSeed.CalendarAsync(context, Caller), "Caller's");
        var otherPublic = await PlannerSeed.TaskAsync(context, await PlannerSeed.CalendarAsync(context, Owner, isPublic: true), "Public");
        var otherPrivate = await PlannerSeed.TaskAsync(context, await PlannerSeed.CalendarAsync(context, Owner), "Owner's private");
        return (own, otherPublic, otherPrivate);
    }
}
