using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.PlannerTasks.Dtos;
using Domain.Common;
using Mediator;

namespace Application.PlannerTasks.Commands.CompletePlannerTask;

/// <summary>
/// Command to mark a planner task as completed or incomplete.
/// </summary>
public class CompletePlannerTaskCommand : IRequest<BaseResponse<PlannerTaskDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the planner task to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the task should be marked as completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Handles the completion state change of a planner task.
/// </summary>
public class CompletePlannerTaskCommandHandler : IRequestHandler<CompletePlannerTaskCommand, BaseResponse<PlannerTaskDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePlannerTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CompletePlannerTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to change the completion state of a planner task.
    /// </summary>
    /// <param name="request">The command containing the task identifier and completion state.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated planner task DTO.</returns>
    public async Task<BaseResponse<PlannerTaskDto>> Handle(CompletePlannerTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PlannerTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Planner task with id {request.Id} was not found.");
        }

        entity.IsCompleted = request.IsCompleted;

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<PlannerTaskDto>.Ok(
            new PlannerTaskDto(entity),
            request.IsCompleted ? "Planner task marked as completed." : "Planner task marked as incomplete.");
    }
}
