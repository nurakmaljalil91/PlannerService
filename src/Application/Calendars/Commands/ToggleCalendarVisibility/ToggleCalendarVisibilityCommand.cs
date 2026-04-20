#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Mediator;

namespace Application.Calendars.Commands.ToggleCalendarVisibility;

/// <summary>
/// Command to toggle the visibility of a calendar owned by the current user.
/// </summary>
public class ToggleCalendarVisibilityCommand : IRequest<BaseResponse<CalendarDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the calendar whose visibility will be toggled.
    /// </summary>
    public long CalendarId { get; set; }
}

/// <summary>
/// Handles toggling the <see cref="Domain.Entities.Calendar.IsVisible"/> flag for a user-owned calendar.
/// </summary>
public class ToggleCalendarVisibilityCommandHandler : IRequestHandler<ToggleCalendarVisibilityCommand, BaseResponse<CalendarDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleCalendarVisibilityCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public ToggleCalendarVisibilityCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the visibility toggle request.
    /// </summary>
    /// <param name="request">The command containing the calendar identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated calendar DTO.</returns>
    public async Task<BaseResponse<CalendarDto>> Handle(ToggleCalendarVisibilityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calendars.FindAsync(new object[] { request.CalendarId }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Calendar with id {request.CalendarId} was not found.");
        }

        if (entity.UserId != _user.UserId)
        {
            throw new ForbiddenAccessException();
        }

        entity.IsVisible = !entity.IsVisible;

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<CalendarDto>.Ok(
            new CalendarDto(entity),
            $"Calendar {request.CalendarId} visibility set to {entity.IsVisible}.");
    }
}
