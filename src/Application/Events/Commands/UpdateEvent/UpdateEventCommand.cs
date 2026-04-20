#nullable enable
using System;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Events.Dtos;
using Domain.Common;
using FluentValidation;
using Mediator;

namespace Application.Events.Commands.UpdateEvent;

/// <summary>
/// Command to update an existing calendar event.
/// </summary>
public class UpdateEventCommand : IRequest<BaseResponse<EventDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the event to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the new title of the event.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the new description of the event.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the new start time of the event.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the new end time of the event.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the new location of the event.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event spans an entire day.
    /// </summary>
    public bool? IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event is recurring.
    /// </summary>
    public bool? IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets the recurrence rule for the event.
    /// </summary>
    public string? RecurrenceRule { get; set; }
}

/// <summary>
/// Handles the update of an existing calendar event.
/// </summary>
public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, BaseResponse<EventDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEventCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UpdateEventCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the update of an existing calendar event.
    /// </summary>
    /// <param name="request">The command containing the updated event details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated event DTO.</returns>
    public async Task<BaseResponse<EventDto>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Events.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Event with id {request.Id} was not found.");
        }

        entity.Title = request.Title ?? entity.Title;
        entity.Description = request.Description ?? entity.Description;
        entity.Location = request.Location ?? entity.Location;
        entity.RecurrenceRule = request.RecurrenceRule ?? entity.RecurrenceRule;

        if (request.StartTime.HasValue)
        {
            entity.StartTime = request.StartTime.Value;
        }

        if (request.EndTime.HasValue)
        {
            entity.EndTime = request.EndTime.Value;
        }

        if (request.IsAllDay.HasValue)
        {
            entity.IsAllDay = request.IsAllDay.Value;
        }

        if (request.IsRecurring.HasValue)
        {
            entity.IsRecurring = request.IsRecurring.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<EventDto>.Ok(new EventDto(entity), "Event updated successfully.");
    }
}

/// <summary>
/// Validates the <see cref="UpdateEventCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEventCommandValidator"/> class.
    /// </summary>
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.")
            .When(x => x.Location != null);
    }
}
