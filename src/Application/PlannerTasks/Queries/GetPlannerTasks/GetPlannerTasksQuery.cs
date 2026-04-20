using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.PlannerTasks.Dtos;
using Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.PlannerTasks.Queries.GetPlannerTasks;

/// <summary>
/// Represents a paginated query for retrieving planner tasks, with optional filtering by calendar.
/// </summary>
public class GetPlannerTasksQuery : PaginatedRequest, IRequest<BaseResponse<PaginatedEnumerable<PlannerTaskDto>>>
{
    /// <summary>
    /// Gets or sets an optional calendar identifier to filter tasks by.
    /// </summary>
    public long? CalendarId { get; set; }
}

/// <summary>
/// Handles the retrieval of a paginated list of planner tasks.
/// </summary>
public class GetPlannerTasksQueryHandler : IRequestHandler<GetPlannerTasksQuery, BaseResponse<PaginatedEnumerable<PlannerTaskDto>>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlannerTasksQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetPlannerTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a paginated list of planner tasks.
    /// </summary>
    /// <param name="request">The query parameters including optional calendar filter, pagination, sorting, and filtering.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated collection of <see cref="PlannerTaskDto"/> instances.</returns>
    public async Task<BaseResponse<PaginatedEnumerable<PlannerTaskDto>>> Handle(
        GetPlannerTasksQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PlannerTasks.AsQueryable();

        if (request.CalendarId.HasValue)
        {
            query = query.Where(t => t.CalendarId == request.CalendarId.Value);
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
            .Select(t => new PlannerTaskDto(t))
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedEnumerable<PlannerTaskDto>(result, totalCount, page, pageSize);

        return BaseResponse<PaginatedEnumerable<PlannerTaskDto>>.Ok(
            paginatedResult,
            $"Successfully retrieved {paginatedResult.Items?.Count() ?? 0} planner tasks.");
    }
}
