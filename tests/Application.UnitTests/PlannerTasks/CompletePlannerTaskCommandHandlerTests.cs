using Application.Common.Exceptions;
using Application.PlannerTasks.Commands.CompletePlannerTask;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;
using Domain.Enums;

namespace Application.UnitTests.PlannerTasks;

/// <summary>
/// Unit tests for <see cref="CompletePlannerTaskCommandHandler"/>.
/// </summary>
public class CompletePlannerTaskCommandHandlerTests
{
    /// <summary>
    /// Verifies that <see cref="CompletePlannerTaskCommandHandler.Handle"/> throws a <see cref="NotFoundException"/>
    /// when the specified task does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_WhenTaskMissing_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CompletePlannerTaskCommandHandler(context);

        var command = new CompletePlannerTaskCommand { Id = 999, IsCompleted = true };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <see cref="CompletePlannerTaskCommandHandler.Handle"/> sets <c>IsCompleted</c> to <c>true</c>
    /// when the command requests completion.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_SetsIsCompletedToTrue()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Cal", UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var task = new PlannerTask
        {
            Title = "Incomplete Task",
            Priority = PriorityLevel.Low,
            IsCompleted = false,
            CalendarId = calendar.Id
        };
        context.PlannerTasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CompletePlannerTaskCommandHandler(context);
        var result = await handler.Handle(
            new CompletePlannerTaskCommand { Id = task.Id, IsCompleted = true },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data!.IsCompleted);
    }
}
