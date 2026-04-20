using Application.Common.Models;
using Application.PlannerTasks.Commands.CompletePlannerTask;
using Application.PlannerTasks.Commands.CreatePlannerTask;
using Application.PlannerTasks.Commands.DeletePlannerTask;
using Application.PlannerTasks.Commands.UpdatePlannerTask;
using Application.PlannerTasks.Dtos;
using Application.PlannerTasks.Queries.GetPlannerTasks;
using Domain.Common;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// API controller for managing planner tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlannerTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlannerTasksController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling requests.</param>
    public PlannerTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all planner tasks with optional pagination, sorting, filtering, and calendar filter.
    /// </summary>
    /// <param name="query">The query parameters for pagination, sorting, filtering, and optional calendar filter.</param>
    /// <returns>A paginated list of <see cref="PlannerTaskDto"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedEnumerable<PlannerTaskDto>>>> GetPlannerTasks(
        [FromQuery] GetPlannerTasksQuery query)
        => Ok(await _mediator.Send(query));

    /// <summary>
    /// Creates a new planner task.
    /// </summary>
    /// <param name="command">The command containing the planner task details.</param>
    /// <returns>The created <see cref="PlannerTaskDto"/>.</returns>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<PlannerTaskDto>>> CreatePlannerTask(
        [FromBody] CreatePlannerTaskCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPlannerTasks), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Updates an existing planner task.
    /// </summary>
    /// <param name="command">The command containing the updated planner task details.</param>
    /// <returns>The updated <see cref="PlannerTaskDto"/>.</returns>
    [HttpPatch]
    public async Task<ActionResult<BaseResponse<PlannerTaskDto>>> UpdatePlannerTask(
        [FromBody] UpdatePlannerTaskCommand command)
        => Ok(await _mediator.Send(command));

    /// <summary>
    /// Marks a planner task as completed or incomplete.
    /// </summary>
    /// <param name="id">The identifier of the planner task.</param>
    /// <param name="command">The command containing the completion state.</param>
    /// <returns>The updated <see cref="PlannerTaskDto"/>.</returns>
    [HttpPatch("{id:long}/complete")]
    public async Task<ActionResult<BaseResponse<PlannerTaskDto>>> CompletePlannerTask(
        long id,
        [FromBody] CompletePlannerTaskCommand command)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Deletes a planner task by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the planner task to delete.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<BaseResponse<string>>> DeletePlannerTask(long id)
        => Ok(await _mediator.Send(new DeletePlannerTaskCommand { Id = id }));
}
