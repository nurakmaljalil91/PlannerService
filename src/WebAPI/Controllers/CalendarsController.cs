using Application.Calendars.Commands.CreateCalendar;
using Application.Calendars.Commands.DeleteCalendar;
using Application.Calendars.Commands.UpdateCalendar;
using Application.Calendars.Dtos;
using Application.Calendars.Queries.GetCalendarById;
using Application.Calendars.Queries.GetCalendars;
using Application.Common.Models;
using Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// API controller for managing calendars.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling requests.</param>
    public CalendarsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all calendars with optional pagination, sorting, and filtering.
    /// </summary>
    /// <param name="query">The query parameters for pagination, sorting, and filtering.</param>
    /// <returns>A paginated list of <see cref="CalendarDto"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedEnumerable<CalendarDto>>>> GetCalendars(
        [FromQuery] GetCalendarsQuery query)
        => Ok(await _mediator.Send(query));

    /// <summary>
    /// Retrieves a single calendar by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the calendar to retrieve.</param>
    /// <returns>A <see cref="CalendarDto"/> for the specified calendar.</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<BaseResponse<CalendarDto>>> GetCalendarById(long id)
        => Ok(await _mediator.Send(new GetCalendarByIdQuery { Id = id }));

    /// <summary>
    /// Creates a new calendar.
    /// </summary>
    /// <param name="command">The command containing the calendar details.</param>
    /// <returns>The created <see cref="CalendarDto"/>.</returns>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<CalendarDto>>> CreateCalendar(
        [FromBody] CreateCalendarCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCalendarById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Updates an existing calendar.
    /// </summary>
    /// <param name="command">The command containing the updated calendar details.</param>
    /// <returns>The updated <see cref="CalendarDto"/>.</returns>
    [HttpPatch]
    public async Task<ActionResult<BaseResponse<CalendarDto>>> UpdateCalendar(
        [FromBody] UpdateCalendarCommand command)
        => Ok(await _mediator.Send(command));

    /// <summary>
    /// Deletes a calendar by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the calendar to delete.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<BaseResponse<string>>> DeleteCalendar(long id)
        => Ok(await _mediator.Send(new DeleteCalendarCommand { Id = id }));
}
