#nullable enable
using System;
using Domain.Entities;

namespace Application.Reminders.Dtos;

/// <summary>
/// Data Transfer Object representing a reminder for a calendar event.
/// </summary>
public class ReminderDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReminderDto"/> class from a <see cref="Reminder"/> entity.
    /// </summary>
    /// <param name="source">The source <see cref="Reminder"/> entity to map from.</param>
    public ReminderDto(Reminder source)
    {
        Id = source.Id;
        Title = source.Title;
        ReminderDateTime = source.ReminderDateTime;
        IsSent = source.IsSent;
        EventId = source.EventId;
    }

    /// <summary>
    /// Gets the unique identifier of the reminder.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the title of the reminder.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the date and time when the reminder will fire.
    /// </summary>
    public DateTime ReminderDateTime { get; init; }

    /// <summary>
    /// Gets a value indicating whether the reminder has already been sent.
    /// </summary>
    public bool IsSent { get; init; }

    /// <summary>
    /// Gets the identifier of the event to which this reminder belongs.
    /// </summary>
    public long EventId { get; init; }
}
