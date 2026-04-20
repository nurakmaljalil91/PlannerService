#nullable enable
using System;
using Application.Common.Interfaces;
using Application.Events.Dtos;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Mediator;

namespace Application.Events.Commands.CreateEvent;

/// <summary>
/// Command to create a new calendar event.
/// </summary>
public class CreateEventCommand : IRequest<BaseResponse<EventDto>>
{
    /// <summary>
    /// Gets or sets the title of the event.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the event.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the start time of the event.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the event.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the location of the event.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event spans an entire day.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event is recurring.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets the recurrence rule for the event.
    /// </summary>
    public string? RecurrenceRule { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the calendar to which this event belongs.
    /// </summary>
    public long CalendarId { get; set; }
}

/// <summary>
/// Handles the creation of a new calendar event.
/// </summary>
public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, BaseResponse<EventDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEventCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CreateEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the creation of a new calendar event.
    /// </summary>
    /// <param name="request">The command containing the event details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the created event DTO.</returns>
    public async Task<BaseResponse<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Location = request.Location,
            IsAllDay = request.IsAllDay,
            IsRecurring = request.IsRecurring,
            RecurrenceRule = request.RecurrenceRule,
            CalendarId = request.CalendarId
        };

        _context.Events.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EventDto(entity);

        return BaseResponse<EventDto>.Ok(dto, $"Created event with id {dto.Id}");
    }
}

/// <summary>
/// Validates the <see cref="CreateEventCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEventCommandValidator"/> class.
    /// </summary>
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.");

        RuleFor(x => x.CalendarId)
            .GreaterThan(0L).WithMessage("CalendarId must be greater than 0.");

        RuleFor(x => x.EndTime)
            .GreaterThanOrEqualTo(x => x.StartTime)
            .WithMessage("EndTime must be on or after StartTime.");
    }
}
