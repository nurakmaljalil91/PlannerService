#nullable enable
using System;
using Domain.Entities;
using Domain.Enums;

namespace Application.PlannerTasks.Dtos;

/// <summary>
/// Data Transfer Object representing a planner task.
/// </summary>
public class PlannerTaskDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlannerTaskDto"/> class from a <see cref="PlannerTask"/> entity.
    /// </summary>
    /// <param name="source">The source <see cref="PlannerTask"/> entity to map from.</param>
    public PlannerTaskDto(PlannerTask source)
    {
        Id = source.Id;
        Title = source.Title;
        Note = source.Note;
        Priority = source.Priority;
        DueDate = source.DueDate;
        Reminder = source.Reminder;
        IsCompleted = source.IsCompleted;
        CalendarId = source.CalendarId;
    }

    /// <summary>
    /// Gets the unique identifier of the planner task.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the title of the planner task.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the optional note associated with the planner task.
    /// </summary>
    public string? Note { get; init; }

    /// <summary>
    /// Gets the priority level of the planner task.
    /// </summary>
    public PriorityLevel Priority { get; init; }

    /// <summary>
    /// Gets the due date of the planner task.
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the reminder time for the planner task.
    /// </summary>
    public DateTime? Reminder { get; init; }

    /// <summary>
    /// Gets a value indicating whether the planner task has been completed.
    /// </summary>
    public bool IsCompleted { get; init; }

    /// <summary>
    /// Gets the identifier of the calendar to which this task belongs.
    /// </summary>
    public long CalendarId { get; init; }
}
