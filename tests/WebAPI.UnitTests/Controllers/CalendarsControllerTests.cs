#nullable enable
using Application.Calendars.Commands.UpdateCalendar;
using Application.Calendars.Dtos;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.UnitTests.TestInfrastructure;

namespace WebAPI.UnitTests.Controllers;

/// <summary>
/// Unit tests for <see cref="CalendarsController"/>.
/// </summary>
public sealed class CalendarsControllerTests
{
    /// <summary>
    /// Verifies that route id values are copied into update calendar commands.
    /// </summary>
    [Fact]
    public async Task UpdateCalendar_WithRouteId_SendsCommandWithId()
    {
        var response = BaseResponse<CalendarDto>.Ok(new CalendarDto(new Calendar
        {
            Id = 42,
            Title = "Work",
            UserId = Guid.NewGuid()
        }));
        var mediator = new TestMediator(_ => Task.FromResult<object>(response));
        var controller = new CalendarsController(mediator);
        var command = new UpdateCalendarCommand { Color = "#3b82f6" };

        var result = await controller.UpdateCalendar(42, command);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, okResult.Value);
        var sentCommand = Assert.IsType<UpdateCalendarCommand>(mediator.LastRequest);
        Assert.Equal(42, sentCommand.Id);
        Assert.Equal("#3b82f6", sentCommand.Color);
    }
}
