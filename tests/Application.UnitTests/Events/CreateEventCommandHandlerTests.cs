using Application.Calendars.Commands.CreateCalendar;
using Application.Events.Commands.CreateEvent;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Events;

/// <summary>
/// Unit tests for <see cref="CreateEventCommandHandler"/>.
/// </summary>
public class CreateEventCommandHandlerTests
{
    /// <summary>
    /// Verifies that <see cref="CreateEventCommandHandler.Handle"/> creates an event and returns
    /// a successful response containing the created event DTO.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithEventDto()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Test Calendar", UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateEventCommandHandler(context);
        var start = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

        var command = new CreateEventCommand
        {
            Title = "Team Meeting",
            Description = "Weekly sync",
            StartTime = start,
            EndTime = end,
            Location = "Conference Room A",
            CalendarId = calendar.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Team Meeting", result.Data.Title);
        Assert.Equal(start, result.Data.StartTime);
    }
}
