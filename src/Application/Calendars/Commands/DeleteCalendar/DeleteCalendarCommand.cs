using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;

namespace Application.Calendars.Commands.DeleteCalendar;

/// <summary>
/// Command to delete a calendar by its identifier.
/// </summary>
public class DeleteCalendarCommand : IRequest<BaseResponse<string>>
{
    /// <summary>
    /// Gets or sets the identifier of the calendar to delete.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the deletion of a calendar by its identifier.
/// </summary>
public class DeleteCalendarCommandHandler : IRequestHandler<DeleteCalendarCommand, BaseResponse<string>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCalendarCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public DeleteCalendarCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the deletion of a calendar.
    /// </summary>
    /// <param name="request">The command containing the calendar identifier to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response indicating the result of the delete operation.</returns>
    public async Task<BaseResponse<string>> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calendars.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Calendar with id {request.Id} was not found.");
        }

        _context.Calendars.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Calendar with id {request.Id} deleted successfully.");
    }
}
