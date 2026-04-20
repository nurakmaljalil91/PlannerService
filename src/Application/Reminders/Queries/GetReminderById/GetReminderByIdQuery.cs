using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Reminders.Dtos;
using Domain.Common;
using Mediator;

namespace Application.Reminders.Queries.GetReminderById;

/// <summary>
/// Represents a query to retrieve a single reminder by its identifier.
/// </summary>
public class GetReminderByIdQuery : IRequest<BaseResponse<ReminderDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the reminder to retrieve.
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// Handles the retrieval of a single reminder by its identifier.
/// </summary>
public class GetReminderByIdQueryHandler : IRequestHandler<GetReminderByIdQuery, BaseResponse<ReminderDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReminderByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GetReminderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the request to retrieve a reminder by its identifier.
    /// </summary>
    /// <param name="request">The query containing the reminder identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the <see cref="ReminderDto"/> if found.</returns>
    public async Task<BaseResponse<ReminderDto>> Handle(GetReminderByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Reminders.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Reminder with id {request.Id} was not found.");
        }

        return BaseResponse<ReminderDto>.Ok(new ReminderDto(entity));
    }
}
