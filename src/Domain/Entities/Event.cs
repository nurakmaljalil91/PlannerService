#nullable enable
using System;
using System.Collections.Generic;

namespace Domain.Entities;

/// <summary>
/// Represents an event in a calendar.
/// </summary>
public class Event : BaseAuditableEntity
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
    /// Gets or sets a value indicating whether the event is an all-day event.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the event is recurring.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets the recurrence rule for the event (e.g., iCal RRULE).
    /// </summary>
    public string? RecurrenceRule { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated Google Calendar event.
    /// </summary>
    public string? GoogleEventId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the calendar to which this event belongs.
    /// </summary>
    public int CalendarId { get; set; }

    /// <summary>
    /// Gets or sets the calendar to which this event belongs.
    /// </summary>
    public Calendar Calendar { get; set; } = null!;

    /// <summary>
    /// Gets the list of reminders for this event.
    /// </summary>
    public IList<Reminder> Reminders { get; private set; } = new List<Reminder>();
}
