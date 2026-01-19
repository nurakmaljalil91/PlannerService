#nullable enable
using System;
using System.Collections.Generic;

namespace Domain.Entities;

/// <summary>
/// Represents a calendar that can contain events and tasks.
/// </summary>
public class Calendar : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the title of the calendar.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the calendar.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the time zone of the calendar.
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary calendar for the user.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this calendar is synchronized with a Google Calendar.
    /// </summary>
    public bool IsGoogleCalendar { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated Google Calendar.
    /// </summary>
    public string? GoogleCalendarId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who owns the calendar.
    /// </summary>
    public Guid UserId { get; set; } 

    /// <summary>
    /// Gets the list of events associated with this calendar.
    /// </summary>
    public IList<Event> Events { get; private set; } = new List<Event>();

    /// <summary>
    /// Gets the list of tasks associated with this calendar.
    /// </summary>
    public IList<PlannerTask> Tasks { get; private set; } = new List<PlannerTask>();
}
