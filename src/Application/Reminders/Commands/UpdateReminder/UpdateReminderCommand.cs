#nullable enable
using System;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Reminders.Dtos;
using Domain.Common;
using FluentValidation;
using Mediator;

namespace Application.Reminders.Commands.UpdateReminder;

/// <summary>
/// Command to update an existing reminder.
/// </summary>
public class UpdateReminderCommand : IRequest<BaseResponse<ReminderDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the reminder to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the new title for the reminder.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the new date and time for the reminder.
    /// </summary>
    public DateTime? ReminderDateTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reminder has been sent.
    /// </summary>
    public bool? IsSent { get; set; }
}

/// <summary>
/// Handles the update of an existing reminder.
/// </summary>
public class UpdateReminderCommandHandler : IRequestHandler<UpdateReminderCommand, BaseResponse<ReminderDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateReminderCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UpdateReminderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the update of an existing reminder.
    /// </summary>
    /// <param name="request">The command containing the updated reminder details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated reminder DTO.</returns>
    public async Task<BaseResponse<ReminderDto>> Handle(UpdateReminderCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Reminders.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Reminder with id {request.Id} was not found.");
        }

        entity.Title = request.Title ?? entity.Title;

        if (request.ReminderDateTime.HasValue)
        {
            entity.ReminderDateTime = request.ReminderDateTime.Value;
        }

        if (request.IsSent.HasValue)
        {
            entity.IsSent = request.IsSent.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<ReminderDto>.Ok(new ReminderDto(entity), "Reminder updated successfully.");
    }
}

/// <summary>
/// Validates the <see cref="UpdateReminderCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class UpdateReminderCommandValidator : AbstractValidator<UpdateReminderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateReminderCommandValidator"/> class.
    /// </summary>
    public UpdateReminderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title != null);
    }
}
