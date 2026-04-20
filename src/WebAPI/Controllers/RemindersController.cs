using Application.Common.Models;
using Application.Reminders.Commands.CreateReminder;
using Application.Reminders.Commands.DeleteReminder;
using Application.Reminders.Commands.UpdateReminder;
using Application.Reminders.Dtos;
using Application.Reminders.Queries.GetReminderById;
using Application.Reminders.Queries.GetReminders;
using Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// API controller for managing event reminders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemindersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling requests.</param>
    public RemindersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all reminders with optional pagination, sorting, filtering, and event filter.
    /// </summary>
    /// <param name="query">The query parameters for pagination, sorting, filtering, and optional event filter.</param>
    /// <returns>A paginated list of <see cref="ReminderDto"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedEnumerable<ReminderDto>>>> GetReminders(
        [FromQuery] GetRemindersQuery query)
        => Ok(await _mediator.Send(query));

    /// <summary>
    /// Retrieves a single reminder by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the reminder to retrieve.</param>
    /// <returns>A <see cref="ReminderDto"/> for the specified reminder.</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<BaseResponse<ReminderDto>>> GetReminderById(long id)
        => Ok(await _mediator.Send(new GetReminderByIdQuery { Id = id }));

    /// <summary>
    /// Creates a new reminder for a calendar event.
    /// </summary>
    /// <param name="command">The command containing the reminder details.</param>
    /// <returns>The created <see cref="ReminderDto"/>.</returns>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<ReminderDto>>> CreateReminder(
        [FromBody] CreateReminderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetReminderById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Updates an existing reminder.
    /// </summary>
    /// <param name="command">The command containing the updated reminder details.</param>
    /// <returns>The updated <see cref="ReminderDto"/>.</returns>
    [HttpPatch]
    public async Task<ActionResult<BaseResponse<ReminderDto>>> UpdateReminder(
        [FromBody] UpdateReminderCommand command)
        => Ok(await _mediator.Send(command));

    /// <summary>
    /// Deletes a reminder by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the reminder to delete.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<BaseResponse<string>>> DeleteReminder(long id)
        => Ok(await _mediator.Send(new DeleteReminderCommand { Id = id }));
}
