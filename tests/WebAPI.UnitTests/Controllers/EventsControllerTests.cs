#nullable enable
using Application.Events.Commands.UpdateEvent;
using Application.Events.Dtos;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.UnitTests.TestInfrastructure;

namespace WebAPI.UnitTests.Controllers;

/// <summary>
/// Unit tests for <see cref="EventsController"/>.
/// </summary>
public sealed class EventsControllerTests
{
    /// <summary>
    /// Verifies that route id values are copied into update event commands.
    /// </summary>
    [Fact]
    public async Task UpdateEvent_WithRouteId_SendsCommandWithId()
    {
        var response = BaseResponse<EventDto>.Ok(new EventDto(new Event
        {
            Id = 215,
            Title = "Merdeka Day",
            StartTime = new DateTime(2026, 8, 31, 5, 48, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 8, 31, 5, 48, 0, DateTimeKind.Utc),
            CalendarId = 1
        }));
        var mediator = new TestMediator(_ => Task.FromResult<object>(response));
        var controller = new EventsController(mediator);
        var command = new UpdateEventCommand { Title = "Merdeka Day" };

        var result = await controller.UpdateEvent(215, command);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, okResult.Value);
        var sentCommand = Assert.IsType<UpdateEventCommand>(mediator.LastRequest);
        Assert.Equal(215, sentCommand.Id);
        Assert.Equal("Merdeka Day", sentCommand.Title);
    }
}
