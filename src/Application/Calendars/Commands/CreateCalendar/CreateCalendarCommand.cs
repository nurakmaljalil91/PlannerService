#nullable enable
using Application.Calendars.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Mediator;

namespace Application.Calendars.Commands.CreateCalendar;

/// <summary>
/// Command to create a new calendar.
/// </summary>
public class CreateCalendarCommand : IRequest<BaseResponse<CalendarDto>>
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

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary calendar.
    /// </summary>
    public bool IsPrimary { get; set; }
}

/// <summary>
/// Handles the creation of a new calendar. The owning user is derived from the JWT claims.
/// </summary>
public class CreateCalendarCommandHandler : IRequestHandler<CreateCalendarCommand, BaseResponse<CalendarDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IUserServiceClient _userServiceClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCalendarCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="user">The current authenticated user.</param>
    /// <param name="userServiceClient">The UserService client used to validate the owning user.</param>
    public CreateCalendarCommandHandler(IApplicationDbContext context, IUser user, IUserServiceClient userServiceClient)
    {
        _context = context;
        _user = user;
        _userServiceClient = userServiceClient;
    }

    /// <summary>
    /// Handles the creation of a new calendar.
    /// </summary>
    /// <param name="request">The command containing the details for the new calendar.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the created calendar DTO.</returns>
    public async Task<BaseResponse<CalendarDto>> Handle(CreateCalendarCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.UserId ?? throw new UnauthorizedAccessException();

        if (!await _userServiceClient.UserExistsAsync(userId, cancellationToken))
        {
            throw new NotFoundException("User", userId);
        }

        var entity = new Calendar
        {
            Title = request.Title,
            Description = request.Description,
            TimeZone = request.TimeZone,
            IsPrimary = request.IsPrimary,
            UserId = userId
        };

        _context.Calendars.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new CalendarDto(entity);

        return BaseResponse<CalendarDto>.Ok(dto, $"Created calendar with id {dto.Id}");
    }
}

/// <summary>
/// Validates the <see cref="CreateCalendarCommand"/> to ensure required fields are provided.
/// </summary>
public class CreateCalendarCommandValidator : AbstractValidator<CreateCalendarCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCalendarCommandValidator"/> class.
    /// </summary>
    public CreateCalendarCommandValidator()
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
