#nullable enable
using System;
using Domain.Entities;

namespace Application.Calendars.Dtos;

/// <summary>
/// Data Transfer Object representing a calendar.
/// </summary>
public class CalendarDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarDto"/> class from a <see cref="Calendar"/> entity.
    /// </summary>
    /// <param name="source">The source <see cref="Calendar"/> entity to map from.</param>
    public CalendarDto(Calendar source)
    {
        Id = source.Id;
        Title = source.Title;
        Description = source.Description;
        TimeZone = source.TimeZone;
        IsPrimary = source.IsPrimary;
        IsGoogleCalendar = source.IsGoogleCalendar;
        GoogleCalendarId = source.GoogleCalendarId;
        UserId = source.UserId;
        IsPublic = source.IsPublic;
        IsVisible = source.IsVisible;
        Color = source.Color;
    }

    /// <summary>
    /// Gets the unique identifier of the calendar.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the title of the calendar.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the description of the calendar.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the time zone of the calendar.
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is the primary calendar for the user.
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Gets a value indicating whether this calendar is synchronized with Google Calendar.
    /// </summary>
    public bool IsGoogleCalendar { get; init; }

    /// <summary>
    /// Gets the Google Calendar identifier, if applicable.
    /// </summary>
    public string? GoogleCalendarId { get; init; }

    /// <summary>
    /// Gets the identifier of the user who owns the calendar.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets a value indicating whether this calendar is publicly visible to all users.
    /// </summary>
    public bool IsPublic { get; init; }

    /// <summary>
    /// Gets a value indicating whether this calendar is visible to its owner.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Gets the display color for this calendar (hex value, e.g. #3b82f6).
    /// </summary>
    public string? Color { get; init; }
}
