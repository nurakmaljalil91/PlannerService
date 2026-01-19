#nullable enable
using System;

namespace Domain.Entities;

/// <summary>
/// Represents a reminder for an event.
/// </summary>
public class Reminder : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the title of the reminder.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the reminder.
    /// </summary>
    public DateTime ReminderDateTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reminder has been sent.
    /// </summary>
    public bool IsSent { get; set; }

    /// <summary>
    /// Gets or sets the ID of the event to which this reminder belongs.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the event to which this reminder belongs.
    /// </summary>
    public Event Event { get; set; } = null!;
}
