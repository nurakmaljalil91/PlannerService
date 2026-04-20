#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using FluentValidation;
using Mediator;

namespace Application.Calendars.Commands.UpdateCalendar;

/// <summary>
/// Command to update an existing calendar.
/// </summary>
public class UpdateCalendarCommand : IRequest<BaseResponse<CalendarDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the calendar to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the new title for the calendar.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the new description for the calendar.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the new time zone for the calendar.
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary calendar.
    /// </summary>
    public bool? IsPrimary { get; set; }
}

/// <summary>
/// Handles the update of an existing calendar.
/// </summary>
public class UpdateCalendarCommandHandler : IRequestHandler<UpdateCalendarCommand, BaseResponse<CalendarDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCalendarCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public UpdateCalendarCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the update of an existing calendar.
    /// </summary>
    /// <param name="request">The command containing the updated calendar details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated calendar DTO.</returns>
    public async Task<BaseResponse<CalendarDto>> Handle(UpdateCalendarCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Calendars.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Calendar with id {request.Id} was not found.");
        }

        if (entity.UserId != _user.UserId)
        {
            throw new ForbiddenAccessException();
        }

        entity.Title = request.Title ?? entity.Title;
        entity.Description = request.Description ?? entity.Description;
        entity.TimeZone = request.TimeZone ?? entity.TimeZone;

        if (request.IsPrimary.HasValue)
        {
            entity.IsPrimary = request.IsPrimary.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<CalendarDto>.Ok(new CalendarDto(entity), "Calendar updated successfully.");
    }
}

/// <summary>
/// Validates the <see cref="UpdateCalendarCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class UpdateCalendarCommandValidator : AbstractValidator<UpdateCalendarCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCalendarCommandValidator"/> class.
    /// </summary>
    public UpdateCalendarCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.TimeZone)
            .MaximumLength(100).WithMessage("TimeZone must not exceed 100 characters.")
            .When(x => x.TimeZone != null);
    }
}
