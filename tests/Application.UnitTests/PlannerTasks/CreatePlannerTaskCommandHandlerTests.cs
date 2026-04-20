using Application.PlannerTasks.Commands.CreatePlannerTask;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;
using Domain.Enums;

namespace Application.UnitTests.PlannerTasks;

/// <summary>
/// Unit tests for <see cref="CreatePlannerTaskCommandHandler"/>.
/// </summary>
public class CreatePlannerTaskCommandHandlerTests
{
    /// <summary>
    /// Verifies that <see cref="CreatePlannerTaskCommandHandler.Handle"/> creates a planner task and
    /// returns a successful response containing the created task DTO.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithPlannerTaskDto()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Test Calendar", UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreatePlannerTaskCommandHandler(context);
        var dueDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        var command = new CreatePlannerTaskCommand
        {
            Title = "Write unit tests",
            Note = "Cover all handler paths",
            Priority = PriorityLevel.High,
            DueDate = dueDate,
            CalendarId = calendar.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Write unit tests", result.Data.Title);
        Assert.Equal(PriorityLevel.High, result.Data.Priority);
        Assert.Equal(dueDate, result.Data.DueDate);
        Assert.False(result.Data.IsCompleted);
    }
}
