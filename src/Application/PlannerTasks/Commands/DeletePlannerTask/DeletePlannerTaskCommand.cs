using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
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
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlannerTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public DeletePlannerTaskCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
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

        await _context.EnsureCalendarWritableAsync(
            entity.CalendarId,
            _user.UserId,
            $"Planner task with id {request.Id} was not found.",
            cancellationToken);

        _context.PlannerTasks.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Planner task with id {request.Id} deleted successfully.");
    }
}
