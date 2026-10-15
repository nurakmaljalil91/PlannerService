using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
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
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEventCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public DeleteEventCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
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

        await _context.EnsureCalendarWritableAsync(
            entity.CalendarId,
            _user.UserId,
            $"Event with id {request.Id} was not found.",
            cancellationToken);

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Event with id {request.Id} deleted successfully.");
    }
}
