using Application.Common.Exceptions;
using Application.Reminders.Commands.DeleteReminder;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Reminders;

/// <summary>
/// Unit tests for <see cref="DeleteReminderCommandHandler"/>.
/// </summary>
public class DeleteReminderCommandHandlerTests
{
    /// <summary>
    /// Verifies that <see cref="DeleteReminderCommandHandler.Handle"/> throws a <see cref="NotFoundException"/>
    /// when attempting to delete a reminder that does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_WhenReminderMissing_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new DeleteReminderCommandHandler(context);

        var command = new DeleteReminderCommand { Id = 999 };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <see cref="DeleteReminderCommandHandler.Handle"/> successfully deletes
    /// an existing reminder and returns a successful response.
    /// </summary>
    [Fact]
    public async Task Handle_ExistingReminder_DeletesAndReturnsSuccess()
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

        var reminder = new Reminder
        {
            Title = "To Delete",
            ReminderDateTime = DateTime.UtcNow,
            EventId = evt.Id
        };
        context.Reminders.Add(reminder);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteReminderCommandHandler(context);
        var result = await handler.Handle(new DeleteReminderCommand { Id = reminder.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(await context.Reminders.FindAsync(reminder.Id));
    }
}
