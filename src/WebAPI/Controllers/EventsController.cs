using Application.Common.Models;
using Application.Events.Commands.CreateEvent;
using Application.Events.Commands.DeleteEvent;
using Application.Events.Commands.UpdateEvent;
using Application.Events.Dtos;
using Application.Events.Queries.GetEventById;
using Application.Events.Queries.GetEvents;
using Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// API controller for managing calendar events.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling requests.</param>
    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all calendar events with optional pagination, sorting, filtering, and calendar filter.
    /// </summary>
    /// <param name="query">The query parameters for pagination, sorting, filtering, and optional calendar filter.</param>
    /// <returns>A paginated list of <see cref="EventDto"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedEnumerable<EventDto>>>> GetEvents(
        [FromQuery] GetEventsQuery query)
        => Ok(await _mediator.Send(query));

    /// <summary>
    /// Retrieves a single calendar event by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the event to retrieve.</param>
    /// <returns>An <see cref="EventDto"/> for the specified event.</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<BaseResponse<EventDto>>> GetEventById(long id)
        => Ok(await _mediator.Send(new GetEventByIdQuery { Id = id }));

    /// <summary>
    /// Creates a new calendar event.
    /// </summary>
    /// <param name="command">The command containing the event details.</param>
    /// <returns>The created <see cref="EventDto"/>.</returns>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<EventDto>>> CreateEvent(
        [FromBody] CreateEventCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEventById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Updates an existing calendar event.
    /// </summary>
    /// <param name="command">The command containing the updated event details.</param>
    /// <returns>The updated <see cref="EventDto"/>.</returns>
    [HttpPatch]
    public async Task<ActionResult<BaseResponse<EventDto>>> UpdateEvent(
        [FromBody] UpdateEventCommand command)
        => Ok(await _mediator.Send(command));

    /// <summary>
    /// Deletes a calendar event by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the event to delete.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<BaseResponse<string>>> DeleteEvent(long id)
        => Ok(await _mediator.Send(new DeleteEventCommand { Id = id }));
}
