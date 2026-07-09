#nullable enable
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Events.Dtos;
using Domain.Common;
using Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Events.Queries.GetEvents;

/// <summary>
/// Represents a paginated query for retrieving calendar events, with optional filtering by calendar.
/// </summary>
public class GetEventsQuery : PaginatedRequest, IRequest<BaseResponse<PaginatedEnumerable<EventDto>>>
{
    /// <summary>
    /// Gets or sets an optional calendar identifier to filter events by.
    /// </summary>
    public long? CalendarId { get; set; }

    /// <summary>
    /// Gets or sets the optional inclusive start of the visible range to filter events by.
    /// </summary>
    public DateTime? RangeStart { get; set; }

    /// <summary>
    /// Gets or sets the optional exclusive end of the visible range to filter events by.
    /// </summary>
    public DateTime? RangeEnd { get; set; }
}

/// <summary>
/// Handles the retrieval of a paginated list of calendar events.
/// </summary>
public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, BaseResponse<PaginatedEnumerable<EventDto>>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEventsQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a paginated list of calendar events.
    /// </summary>
    /// <param name="request">The query parameters including optional calendar filter, pagination, sorting, and filtering.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated collection of <see cref="EventDto"/> instances.</returns>
    public async Task<BaseResponse<PaginatedEnumerable<EventDto>>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Events.AsQueryable();

        if (request.CalendarId.HasValue)
        {
            query = query.Where(e => e.CalendarId == request.CalendarId.Value);
        }

        if (request.RangeStart.HasValue && request.RangeEnd.HasValue)
        {
            var rangeStart = request.RangeStart.Value;
            var rangeEnd = request.RangeEnd.Value;

            query = query.Where(e =>
                (!e.IsRecurring && e.StartTime < rangeEnd && e.EndTime >= rangeStart) ||
                (e.IsRecurring && e.StartTime < rangeEnd));
        }

        query = query
            .ApplyFilters(request.Filter)
            .ApplySorting(request.SortBy, request.Descending);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.Total <= 0 ? 10 : request.Total;

        if (request.RangeStart.HasValue && request.RangeEnd.HasValue)
        {
            var rangeStart = request.RangeStart.Value;
            var rangeEnd = request.RangeEnd.Value;
            var rangeFilteredEvents = await query
                .ToListAsync(cancellationToken);

            var filteredEvents = rangeFilteredEvents
                .Where(e => IsRelevantForRange(e, rangeStart, rangeEnd))
                .ToList();

            var rangeTotalCount = filteredEvents.Count;
            var rangeResult = filteredEvents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventDto(e))
                .ToList();

            var rangePaginatedResult = new PaginatedEnumerable<EventDto>(rangeResult, rangeTotalCount, page, pageSize);

            return BaseResponse<PaginatedEnumerable<EventDto>>.Ok(
                rangePaginatedResult,
                $"Successfully retrieved {rangePaginatedResult.Items?.Count() ?? 0} events.");
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventDto(e))
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedEnumerable<EventDto>(result, totalCount, page, pageSize);

        return BaseResponse<PaginatedEnumerable<EventDto>>.Ok(
            paginatedResult,
            $"Successfully retrieved {paginatedResult.Items?.Count() ?? 0} events.");
    }

    private static bool IsRelevantForRange(Event calendarEvent, DateTime rangeStart, DateTime rangeEnd)
    {
        if (!calendarEvent.IsRecurring)
        {
            return calendarEvent.StartTime < rangeEnd && calendarEvent.EndTime >= rangeStart;
        }

        if (calendarEvent.StartTime >= rangeEnd)
        {
            return false;
        }

        var recurrenceUntil = ParseRecurrenceUntil(calendarEvent.RecurrenceRule);
        return !recurrenceUntil.HasValue || recurrenceUntil.Value >= rangeStart;
    }

    private static DateTime? ParseRecurrenceUntil(string? recurrenceRule)
    {
        if (string.IsNullOrWhiteSpace(recurrenceRule))
        {
            return null;
        }

        const string untilToken = "UNTIL=";
        var untilStart = recurrenceRule.IndexOf(untilToken, StringComparison.OrdinalIgnoreCase);
        if (untilStart < 0)
        {
            return null;
        }

        var valueStart = untilStart + untilToken.Length;
        var valueEnd = recurrenceRule.IndexOf(';', valueStart);
        var value = valueEnd >= 0
            ? recurrenceRule[valueStart..valueEnd]
            : recurrenceRule[valueStart..];

        return DateTime.TryParseExact(
            value,
            "yyyyMMdd'T'HHmmss'Z'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
                ? parsed
                : null;
    }
}
