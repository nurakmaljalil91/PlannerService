using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;

namespace Application.Calendars.Queries.GetCalendarById;

/// <summary>
/// Represents a query to retrieve a single calendar by its identifier.
/// </summary>
public class GetCalendarByIdQuery : IRequest<BaseResponse<CalendarDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the calendar to retrieve.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the retrieval of a single calendar by its identifier.
/// </summary>
public class GetCalendarByIdQueryHandler : IRequestHandler<GetCalendarByIdQuery, BaseResponse<CalendarDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCalendarByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetCalendarByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a calendar by its identifier.
    /// </summary>
    /// <param name="request">The query containing the calendar identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the <see cref="CalendarDto"/> if found.</returns>
    public async Task<BaseResponse<CalendarDto>> Handle(GetCalendarByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calendars.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Calendar with id {request.Id} was not found.");
        }

        return BaseResponse<CalendarDto>.Ok(new CalendarDto(entity));
    }
}
