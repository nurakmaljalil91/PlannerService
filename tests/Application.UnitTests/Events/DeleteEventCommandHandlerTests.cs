using Application.Common.Exceptions;
using Application.Events.Commands.DeleteEvent;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Events;

/// <summary>
/// Unit tests for <see cref="DeleteEventCommandHandler"/>.
/// </summary>
public class DeleteEventCommandHandlerTests
{
    /// <summary>
    /// Verifies that <see cref="DeleteEventCommandHandler.Handle"/> throws a <see cref="NotFoundException"/>
    /// when attempting to delete an event that does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_WhenEventMissing_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new DeleteEventCommandHandler(context);

        var command = new DeleteEventCommand { Id = 999 };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <see cref="DeleteEventCommandHandler.Handle"/> successfully deletes
    /// an existing event and returns a successful response.
    /// </summary>
    [Fact]
    public async Task Handle_ExistingEvent_DeletesAndReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Cal", UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var evt = new Event
        {
            Title = "To Delete",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            CalendarId = calendar.Id
        };
        context.Events.Add(evt);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteEventCommandHandler(context);
        var result = await handler.Handle(new DeleteEventCommand { Id = evt.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(await context.Events.FindAsync(evt.Id));
    }
}
