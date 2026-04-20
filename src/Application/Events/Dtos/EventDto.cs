#nullable enable
using System;
using Domain.Entities;

namespace Application.Events.Dtos;

/// <summary>
/// Data Transfer Object representing a calendar event.
/// </summary>
public class EventDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventDto"/> class from an <see cref="Event"/> entity.
    /// </summary>
    /// <param name="source">The source <see cref="Event"/> entity to map from.</param>
    public EventDto(Event source)
    {
        Id = source.Id;
        Title = source.Title;
        Description = source.Description;
        StartTime = source.StartTime;
        EndTime = source.EndTime;
        Location = source.Location;
        IsAllDay = source.IsAllDay;
        IsRecurring = source.IsRecurring;
        RecurrenceRule = source.RecurrenceRule;
        GoogleEventId = source.GoogleEventId;
        CalendarId = source.CalendarId;
    }

    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the title of the event.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the description of the event.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the start time of the event.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Gets the end time of the event.
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Gets the location of the event.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Gets a value indicating whether the event spans an entire day.
    /// </summary>
    public bool IsAllDay { get; init; }

    /// <summary>
    /// Gets a value indicating whether the event is recurring.
    /// </summary>
    public bool IsRecurring { get; init; }

    /// <summary>
    /// Gets the recurrence rule for the event (e.g., iCal RRULE format).
    /// </summary>
    public string? RecurrenceRule { get; init; }

    /// <summary>
    /// Gets the associated Google Calendar event identifier.
    /// </summary>
    public string? GoogleEventId { get; init; }

    /// <summary>
    /// Gets the identifier of the calendar to which this event belongs.
    /// </summary>
    public long CalendarId { get; init; }
}
