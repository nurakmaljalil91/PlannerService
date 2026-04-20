#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Mediator;

namespace Application.Calendars.Commands.CreatePublicCalendar;

/// <summary>
/// Command to create a new public (admin-managed shareable) calendar.
/// </summary>
public class CreatePublicCalendarCommand : IRequest<BaseResponse<CalendarDto>>
{
    /// <summary>
    /// Gets or sets the title of the calendar.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the calendar.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the time zone of the calendar.
    /// </summary>
    public string? TimeZone { get; set; }
}

/// <summary>
/// Handles the creation of a new public calendar. Only users with the Admin role may call this.
/// </summary>
public class CreatePublicCalendarCommandHandler : IRequestHandler<CreatePublicCalendarCommand, BaseResponse<CalendarDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePublicCalendarCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    public CreatePublicCalendarCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// Handles the creation of a new public calendar.
    /// </summary>
    /// <param name="request">The command containing the calendar details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the created calendar DTO.</returns>
    public async Task<BaseResponse<CalendarDto>> Handle(CreatePublicCalendarCommand request, CancellationToken cancellationToken)
    {
        if (!_user.GetRoles().Contains("Admin"))
        {
            throw new ForbiddenAccessException();
        }

        var entity = new Calendar
        {
            Title = request.Title,
            Description = request.Description,
            TimeZone = request.TimeZone,
            IsPublic = true,
            IsVisible = true,
            UserId = _user.UserId ?? Guid.Empty
        };

        _context.Calendars.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CalendarDto(entity);

        return BaseResponse<CalendarDto>.Ok(dto, $"Created public calendar with id {dto.Id}");
    }
}

/// <summary>
/// Validates the <see cref="CreatePublicCalendarCommand"/> to ensure required fields are provided.
/// </summary>
public class CreatePublicCalendarCommandValidator : AbstractValidator<CreatePublicCalendarCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePublicCalendarCommandValidator"/> class.
    /// </summary>
    public CreatePublicCalendarCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.TimeZone)
            .MaximumLength(100).WithMessage("TimeZone must not exceed 100 characters.");
    }
}
