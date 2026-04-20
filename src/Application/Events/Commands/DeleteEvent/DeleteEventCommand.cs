using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;

namespace Application.Events.Commands.DeleteEvent;

/// <summary>
/// Command to delete a calendar event by its identifier.
/// </summary>
public class DeleteEventCommand : IRequest<BaseResponse<string>>
{
    /// <summary>
    /// Gets or sets the identifier of the event to delete.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the deletion of a calendar event by its identifier.
/// </summary>
public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, BaseResponse<string>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEventCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeleteEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the deletion of a calendar event.
    /// </summary>
    /// <param name="request">The command containing the event identifier to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    public async Task<BaseResponse<string>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Event with id {request.Id} was not found.");
        }

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Event with id {request.Id} deleted successfully.");
    }
}
