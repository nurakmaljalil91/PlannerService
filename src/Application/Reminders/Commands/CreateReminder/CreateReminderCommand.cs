#nullable enable
using System;
using Application.Common.Interfaces;
using Application.Reminders.Dtos;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Mediator;

namespace Application.Reminders.Commands.CreateReminder;

/// <summary>
/// Command to create a new reminder for a calendar event.
/// </summary>
public class CreateReminderCommand : IRequest<BaseResponse<ReminderDto>>
{
    /// <summary>
    /// Gets or sets the title of the reminder.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the reminder should fire.
    /// </summary>
    public DateTime ReminderDateTime { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the event to which this reminder belongs.
    /// </summary>
    public long EventId { get; set; }
}

/// <summary>
/// Handles the creation of a new reminder.
/// </summary>
public class CreateReminderCommandHandler : IRequestHandler<CreateReminderCommand, BaseResponse<ReminderDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateReminderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CreateReminderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the creation of a new reminder.
    /// </summary>
    /// <param name="request">The command containing the reminder details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the created reminder DTO.</returns>
    public async Task<BaseResponse<ReminderDto>> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        var entity = new Reminder
        {
            Title = request.Title,
            ReminderDateTime = request.ReminderDateTime,
            EventId = request.EventId
        };

        _context.Reminders.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ReminderDto(entity);

        return BaseResponse<ReminderDto>.Ok(dto, $"Created reminder with id {dto.Id}");
    }
}

/// <summary>
/// Validates the <see cref="CreateReminderCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class CreateReminderCommandValidator : AbstractValidator<CreateReminderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateReminderCommandValidator"/> class.
    /// </summary>
    public CreateReminderCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.EventId)
            .GreaterThan(0L).WithMessage("EventId must be greater than 0.");

        RuleFor(x => x.ReminderDateTime)
            .NotEmpty().WithMessage("ReminderDateTime is required.");
    }
}
