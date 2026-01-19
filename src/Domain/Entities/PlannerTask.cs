#nullable enable
using System;

namespace Domain.Entities;

/// <summary>
/// Represents a task in a calendar.
/// </summary>
public class PlannerTask : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the title of the task.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional note for the task.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the priority level of the task.
    /// </summary>
    public PriorityLevel Priority { get; set; }

    /// <summary>
    /// Gets or sets the due date of the task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the reminder time for the task.
    /// </summary>
    public DateTime? Reminder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the task is completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets the ID of the calendar to which this task belongs.
    /// </summary>
    public int CalendarId { get; set; }

    /// <summary>
    /// Gets or sets the calendar to which this task belongs.
    /// </summary>
    public Calendar Calendar { get; set; } = null!;
}
