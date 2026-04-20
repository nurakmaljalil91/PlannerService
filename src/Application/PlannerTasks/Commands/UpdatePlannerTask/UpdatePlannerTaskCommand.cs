#nullable enable
using System;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.PlannerTasks.Dtos;
using Domain.Common;
using Domain.Enums;
using FluentValidation;
using Mediator;

namespace Application.PlannerTasks.Commands.UpdatePlannerTask;

/// <summary>
/// Command to update an existing planner task.
/// </summary>
public class UpdatePlannerTaskCommand : IRequest<BaseResponse<PlannerTaskDto>>
{
    /// <summary>
    /// Gets or sets the identifier of the planner task to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the new title of the planner task.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the new note for the planner task.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the new priority level for the planner task.
    /// </summary>
    public PriorityLevel? Priority { get; set; }

    /// <summary>
    /// Gets or sets the new due date for the planner task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the new reminder time for the planner task.
    /// </summary>
    public DateTime? Reminder { get; set; }
}

/// <summary>
/// Handles the update of an existing planner task.
/// </summary>
public class UpdatePlannerTaskCommandHandler : IRequestHandler<UpdatePlannerTaskCommand, BaseResponse<PlannerTaskDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlannerTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UpdatePlannerTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the update of an existing planner task.
    /// </summary>
    /// <param name="request">The command containing the updated task details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the updated planner task DTO.</returns>
    public async Task<BaseResponse<PlannerTaskDto>> Handle(UpdatePlannerTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PlannerTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Planner task with id {request.Id} was not found.");
        }

        entity.Title = request.Title ?? entity.Title;
        entity.Note = request.Note ?? entity.Note;
        entity.DueDate = request.DueDate ?? entity.DueDate;
        entity.Reminder = request.Reminder ?? entity.Reminder;

        if (request.Priority.HasValue)
        {
            entity.Priority = request.Priority.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return BaseResponse<PlannerTaskDto>.Ok(new PlannerTaskDto(entity), "Planner task updated successfully.");
    }
}

/// <summary>
/// Validates the <see cref="UpdatePlannerTaskCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class UpdatePlannerTaskCommandValidator : AbstractValidator<UpdatePlannerTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlannerTaskCommandValidator"/> class.
    /// </summary>
    public UpdatePlannerTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
            .When(x => x.Note != null);
    }
}
