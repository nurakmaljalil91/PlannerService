#nullable enable
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Calendars.Commands.UnsubscribeFromCalendar;

/// <summary>
/// Command to unsubscribe the current user from a calendar.
/// </summary>
public class UnsubscribeFromCalendarCommand : IRequest<BaseResponse<string>>
{
    /// <summary>
    /// Gets or sets the identifier of the calendar to unsubscribe from.
    /// </summary>
    public long CalendarId { get; set; }
}

/// <summary>
/// Handles unsubscribing the current user from a calendar.
/// </summary>
public class UnsubscribeFromCalendarCommandHandler : IRequestHandler<UnsubscribeFromCalendarCommand, BaseResponse<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeFromCalendarCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public UnsubscribeFromCalendarCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the unsubscription request.
    /// </summary>
    /// <param name="request">The command containing the calendar identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response indicating the result.</returns>
    public async Task<BaseResponse<string>> Handle(UnsubscribeFromCalendarCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId ?? Guid.Empty;

        var subscription = await _context.CalendarSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CalendarId == request.CalendarId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException($"Subscription to calendar {request.CalendarId} was not found.");
        }

        _context.CalendarSubscriptions.Remove(subscription);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<string>.Ok($"Successfully unsubscribed from calendar {request.CalendarId}.");
    }
}
