#nullable enable
using System;
using Application.Common.Interfaces;
using Application.PlannerTasks.Dtos;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Mediator;

namespace Application.PlannerTasks.Commands.CreatePlannerTask;

/// <summary>
/// Command to create a new planner task.
/// </summary>
public class CreatePlannerTaskCommand : IRequest<BaseResponse<PlannerTaskDto>>
{
    /// <summary>
    /// Gets or sets the title of the planner task.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional note for the planner task.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the priority level of the planner task.
    /// </summary>
    public PriorityLevel Priority { get; set; }

    /// <summary>
    /// Gets or sets the optional due date of the planner task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the optional reminder time for the planner task.
    /// </summary>
    public DateTime? Reminder { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the calendar to which this task belongs.
    /// </summary>
    public long CalendarId { get; set; }
}

/// <summary>
/// Handles the creation of a new planner task.
/// </summary>
public class CreatePlannerTaskCommandHandler : IRequestHandler<CreatePlannerTaskCommand, BaseResponse<PlannerTaskDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlannerTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CreatePlannerTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the creation of a new planner task.
    /// </summary>
    /// <param name="request">The command containing the task details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A response containing the created planner task DTO.</returns>
    public async Task<BaseResponse<PlannerTaskDto>> Handle(CreatePlannerTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = new PlannerTask
        {
            Title = request.Title,
            Note = request.Note,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Reminder = request.Reminder,
            CalendarId = request.CalendarId
        };

        _context.PlannerTasks.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new PlannerTaskDto(entity);

        return BaseResponse<PlannerTaskDto>.Ok(dto, $"Created planner task with id {dto.Id}");
    }
}

/// <summary>
/// Validates the <see cref="CreatePlannerTaskCommand"/> to ensure required fields are provided correctly.
/// </summary>
public class CreatePlannerTaskCommandValidator : AbstractValidator<CreatePlannerTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlannerTaskCommandValidator"/> class.
    /// </summary>
    public CreatePlannerTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");

        RuleFor(x => x.CalendarId)
            .GreaterThan(0L).WithMessage("CalendarId must be greater than 0.");
    }
}
