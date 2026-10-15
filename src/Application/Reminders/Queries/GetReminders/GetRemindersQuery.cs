using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Common.Models;
using Application.Reminders.Dtos;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Reminders.Queries.GetReminders;

/// <summary>
/// Represents a paginated query for retrieving reminders, with optional filtering by event.
/// </summary>
public class GetRemindersQuery : PaginatedRequest, IRequest<BaseResponse<PaginatedEnumerable<ReminderDto>>>
{
    /// <summary>
    /// Gets or sets an optional event identifier to filter reminders by.
    /// </summary>
    public long? EventId { get; set; }
}

/// <summary>
/// Handles the retrieval of a paginated list of reminders.
/// </summary>
public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, BaseResponse<PaginatedEnumerable<ReminderDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRemindersQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public GetRemindersQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the request to retrieve a paginated list of reminders.
    /// </summary>
    /// <param name="request">The query parameters including optional event filter, pagination, sorting, and filtering.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated collection of <see cref="ReminderDto"/> instances.</returns>
    public async Task<BaseResponse<PaginatedEnumerable<ReminderDto>>> Handle(
        GetRemindersQuery request,
        CancellationToken cancellationToken)
    {
        var calendarIds = request.EventId.HasValue
            ? _context.Calendars.ReadableBy(_user.UserId).Select(c => c.Id)
            : _context.ListedCalendarIds(_user.UserId, calendarId: null);
        var eventIds = _context.Events
            .Where(e => calendarIds.Contains(e.CalendarId))
            .Select(e => e.Id);
        var query = _context.Reminders.Where(r => eventIds.Contains(r.EventId));

        if (request.EventId.HasValue)
        {
            query = query.Where(r => r.EventId == request.EventId.Value);
        }

        query = query
            .ApplyFilters(request.Filter)
            .ApplySorting(request.SortBy, request.Descending);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.Total <= 0 ? 10 : request.Total;

        var totalCount = await query.CountAsync(cancellationToken);

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReminderDto(r))
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedEnumerable<ReminderDto>(result, totalCount, page, pageSize);

        return BaseResponse<PaginatedEnumerable<ReminderDto>>.Ok(
            paginatedResult,
            $"Successfully retrieved {paginatedResult.Items?.Count() ?? 0} reminders.");
    }
}
