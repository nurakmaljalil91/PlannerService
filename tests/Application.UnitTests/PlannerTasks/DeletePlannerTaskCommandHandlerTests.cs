using Application.Common.Exceptions;
using Application.PlannerTasks.Commands.DeletePlannerTask;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;
using Domain.Enums;

namespace Application.UnitTests.PlannerTasks;

/// <summary>
/// Unit tests for <see cref="DeletePlannerTaskCommandHandler"/>.
/// </summary>
public class DeletePlannerTaskCommandHandlerTests
{
    private static readonly Guid Owner = Guid.NewGuid();

    /// <summary>
    /// Verifies that <see cref="DeletePlannerTaskCommandHandler.Handle"/> throws a <see cref="NotFoundException"/>
    /// when attempting to delete a planner task that does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_WhenTaskMissing_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new DeletePlannerTaskCommandHandler(context, new TestUser(Owner));

        var command = new DeletePlannerTaskCommand { Id = 999 };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <see cref="DeletePlannerTaskCommandHandler.Handle"/> successfully deletes
    /// an existing planner task and returns a successful response.
    /// </summary>
    [Fact]
    public async Task Handle_ExistingTask_DeletesAndReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Cal", UserId = Owner };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var task = new PlannerTask
        {
            Title = "To Delete",
            Priority = PriorityLevel.None,
            CalendarId = calendar.Id
        };
        context.PlannerTasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeletePlannerTaskCommandHandler(context, new TestUser(Owner));
        var result = await handler.Handle(new DeletePlannerTaskCommand { Id = task.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(await context.PlannerTasks.FindAsync(task.Id));
    }
}
