using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;

namespace Application.PlannerTasks.Commands.DeletePlannerTask;

/// <summary>
/// Command to delete a planner task by its identifier.
/// </summary>
public class DeletePlannerTaskCommand : IRequest<BaseResponse<string>>
{
    /// <summary>
    /// Gets or sets the identifier of the planner task to delete.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the deletion of a planner task by its identifier.
/// </summary>
public class DeletePlannerTaskCommandHandler : IRequestHandler<DeletePlannerTaskCommand, BaseResponse<string>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlannerTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeletePlannerTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the deletion of a planner task.
    /// </summary>
    /// <param name="request">The command containing the planner task identifier to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    public async Task<BaseResponse<string>> Handle(DeletePlannerTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PlannerTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Planner task with id {request.Id} was not found.");
        }

        _context.PlannerTasks.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Planner task with id {request.Id} deleted successfully.");
    }
}
