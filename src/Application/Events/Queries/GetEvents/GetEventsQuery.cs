using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Events.Dtos;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

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

        query = query
            .ApplyFilters(request.Filter)
            .ApplySorting(request.SortBy, request.Descending);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.Total <= 0 ? 10 : request.Total;

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
}
