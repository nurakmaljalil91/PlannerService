using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Events.Dtos;
using Domain.Common;
using Mediator;

namespace Application.Events.Queries.GetEventById;

/// <summary>
/// Represents a query to retrieve a single calendar event by its identifier.
/// </summary>
public class GetEventByIdQuery : IRequest<BaseResponse<EventDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the event to retrieve.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the retrieval of a single calendar event by its identifier.
/// </summary>
public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, BaseResponse<EventDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEventByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetEventByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a calendar event by its identifier.
    /// </summary>
    /// <param name="request">The query containing the event identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the <see cref="EventDto"/> if found.</returns>
    public async Task<BaseResponse<EventDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Event with id {request.Id} was not found.");
        }

        return BaseResponse<EventDto>.Ok(new EventDto(entity));
    }
}
