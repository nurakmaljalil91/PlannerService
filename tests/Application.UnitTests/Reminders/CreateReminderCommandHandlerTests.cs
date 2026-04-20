using Application.Reminders.Commands.CreateReminder;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Reminders;

/// <summary>
/// Unit tests for <see cref="CreateReminderCommandHandler"/>.
/// </summary>
public class CreateReminderCommandHandlerTests
{
    /// <summary>
    /// Verifies that <see cref="CreateReminderCommandHandler.Handle"/> creates a reminder and
    /// returns a successful response containing the created reminder DTO.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithReminderDto()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Cal", UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var evt = new Event
        {
            Title = "Meeting",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            CalendarId = calendar.Id
        };
        context.Events.Add(evt);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateReminderCommandHandler(context);
        var reminderTime = new DateTime(2026, 5, 1, 8, 45, 0, DateTimeKind.Utc);

        var command = new CreateReminderCommand
        {
            Title = "15 min reminder",
            ReminderDateTime = reminderTime,
            EventId = evt.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("15 min reminder", result.Data.Title);
        Assert.Equal(reminderTime, result.Data.ReminderDateTime);
        Assert.False(result.Data.IsSent);
    }
}
