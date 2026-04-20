using Application.Calendars.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Calendars.Queries.GetCalendars;

/// <summary>
/// Represents a paginated query for retrieving calendars.
/// </summary>
public class GetCalendarsQuery : PaginatedRequest, IRequest<BaseResponse<PaginatedEnumerable<CalendarDto>>>;

/// <summary>
/// Handles the retrieval of a paginated list of calendars.
/// </summary>
public class GetCalendarsQueryHandler : IRequestHandler<GetCalendarsQuery, BaseResponse<PaginatedEnumerable<CalendarDto>>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCalendarsQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetCalendarsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a paginated list of calendars.
    /// </summary>
    /// <param name="request">The query parameters for pagination, sorting, and filtering.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated collection of <see cref="CalendarDto"/> instances.</returns>
    public async Task<BaseResponse<PaginatedEnumerable<CalendarDto>>> Handle(
        GetCalendarsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Calendars
            .AsQueryable()
            .ApplyFilters(request.Filter)
            .ApplySorting(request.SortBy, request.Descending);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.Total <= 0 ? 10 : request.Total;

        var totalCount = await query.CountAsync(cancellationToken);

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CalendarDto(c))
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedEnumerable<CalendarDto>(result, totalCount, page, pageSize);

        return BaseResponse<PaginatedEnumerable<CalendarDto>>.Ok(
            paginatedResult,
            $"Successfully retrieved {paginatedResult.Items?.Count() ?? 0} calendars.");
    }
}
