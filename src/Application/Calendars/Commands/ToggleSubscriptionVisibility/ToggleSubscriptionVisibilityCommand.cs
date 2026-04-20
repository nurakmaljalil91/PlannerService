#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Calendars.Commands.ToggleSubscriptionVisibility;

/// <summary>
/// Command to toggle the visibility of a calendar subscription owned by the current user.
/// </summary>
public class ToggleSubscriptionVisibilityCommand : IRequest<BaseResponse<CalendarSubscriptionDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the subscribed calendar whose subscription visibility will be toggled.
    /// </summary>
    public long CalendarId { get; set; }
}

/// <summary>
/// Handles toggling the <see cref="Domain.Entities.CalendarSubscription.IsVisible"/> flag for a subscription.
/// </summary>
public class ToggleSubscriptionVisibilityCommandHandler : IRequestHandler<ToggleSubscriptionVisibilityCommand, BaseResponse<CalendarSubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleSubscriptionVisibilityCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public ToggleSubscriptionVisibilityCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the subscription visibility toggle request.
    /// </summary>
    /// <param name="request">The command containing the calendar identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated subscription DTO.</returns>
    public async Task<BaseResponse<CalendarSubscriptionDto>> Handle(ToggleSubscriptionVisibilityCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId ?? Guid.Empty;

        var subscription = await _context.CalendarSubscriptions
            .Include(s => s.Calendar)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CalendarId == request.CalendarId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException($"Subscription to calendar {request.CalendarId} was not found.");
        }

        subscription.IsVisible = !subscription.IsVisible;

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<CalendarSubscriptionDto>.Ok(
            new CalendarSubscriptionDto(subscription),
            $"Subscription to calendar {request.CalendarId} visibility set to {subscription.IsVisible}.");
    }
}
