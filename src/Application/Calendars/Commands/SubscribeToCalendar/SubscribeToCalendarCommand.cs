#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Calendars.Commands.SubscribeToCalendar;

/// <summary>
/// Command to subscribe the current user to a public calendar.
/// </summary>
public class SubscribeToCalendarCommand : IRequest<BaseResponse<CalendarSubscriptionDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the calendar to subscribe to.
    /// </summary>
    public long CalendarId { get; set; }
}

/// <summary>
/// Handles subscribing the current user to a public calendar.
/// </summary>
public class SubscribeToCalendarCommandHandler : IRequestHandler<SubscribeToCalendarCommand, BaseResponse<CalendarSubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeToCalendarCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public SubscribeToCalendarCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the subscription request.
    /// </summary>
    /// <param name="request">The command containing the calendar identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the created subscription DTO.</returns>
    public async Task<BaseResponse<CalendarSubscriptionDto>> Handle(SubscribeToCalendarCommand request, CancellationToken cancellationToken)
    {
        var calendar = await _context.Calendars.FindAsync(new object[] { request.CalendarId }, cancellationToken);

        if (calendar == null)
        {
            throw new NotFoundException($"Calendar with id {request.CalendarId} was not found.");
        }

        if (!calendar.IsPublic)
        {
            throw new ForbiddenAccessException();
        }

        var userId = _user.UserId ?? Guid.Empty;

        var existing = await _context.CalendarSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CalendarId == request.CalendarId, cancellationToken);

        if (existing != null)
        {
            throw new ConflictException("You are already subscribed to this calendar.");
        }

        var subscription = new CalendarSubscription
        {
            UserId = userId,
            CalendarId = request.CalendarId,
            IsVisible = true,
            Calendar = calendar
        };

        _context.CalendarSubscriptions.Add(subscription);

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<CalendarSubscriptionDto>.Ok(
            new CalendarSubscriptionDto(subscription),
            $"Successfully subscribed to calendar {request.CalendarId}.");
    }
}

/// <summary>
/// Validates the <see cref="SubscribeToCalendarCommand"/>.
/// </summary>
public class SubscribeToCalendarCommandValidator : AbstractValidator<SubscribeToCalendarCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscribeToCalendarCommandValidator"/> class.
    /// </summary>
    public SubscribeToCalendarCommandValidator()
    {
        RuleFor(x => x.CalendarId)
            .GreaterThan(0).WithMessage("CalendarId must be greater than 0.");
    }
}
