using Application.Calendars.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Calendars.Queries.GetPublicCalendars;

/// <summary>
/// Represents a paginated query for retrieving public calendars.
/// </summary>
public class GetPublicCalendarsQuery : PaginatedRequest, IRequest<BaseResponse<PaginatedEnumerable<CalendarDto>>>;

/// <summary>
/// Handles the retrieval of a paginated list of public calendars.
/// </summary>
public class GetPublicCalendarsQueryHandler : IRequestHandler<GetPublicCalendarsQuery, BaseResponse<PaginatedEnumerable<CalendarDto>>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublicCalendarsQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetPublicCalendarsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a paginated list of public calendars.
    /// </summary>
    /// <param name="request">The query parameters for pagination, sorting, and filtering.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated collection of public <see cref="CalendarDto"/> instances.</returns>
    public async Task<BaseResponse<PaginatedEnumerable<CalendarDto>>> Handle(
        GetPublicCalendarsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Calendars
            .Where(c => c.IsPublic)
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
            $"Successfully retrieved {paginatedResult.Items?.Count() ?? 0} public calendars.");
    }
}
