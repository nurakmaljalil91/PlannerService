#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Calendars.Queries.GetUserSubscriptions;

/// <summary>
/// Represents a query for retrieving all calendar subscriptions of the current user.
/// </summary>
public class GetUserSubscriptionsQuery : IRequest<BaseResponse<IEnumerable<CalendarSubscriptionDto>>>;

/// <summary>
/// Handles the retrieval of all calendar subscriptions for the current user.
/// </summary>
public class GetUserSubscriptionsQueryHandler : IRequestHandler<GetUserSubscriptionsQuery, BaseResponse<IEnumerable<CalendarSubscriptionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSubscriptionsQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public GetUserSubscriptionsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the request to retrieve all subscriptions of the current user.
    /// </summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of <see cref="CalendarSubscriptionDto"/> instances.</returns>
    public async Task<BaseResponse<IEnumerable<CalendarSubscriptionDto>>> Handle(
        GetUserSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _context.CalendarSubscriptions
            .Where(s => s.UserId == _user.UserId)
            .Include(s => s.Calendar)
            .ToListAsync(cancellationToken);

        var dtos = subscriptions.Select(s => new CalendarSubscriptionDto(s));

        return BaseResponse<IEnumerable<CalendarSubscriptionDto>>.Ok(
            dtos,
            $"Successfully retrieved {subscriptions.Count} subscriptions.");
    }
}
