#nullable enable
using System;
using Domain.Entities;

namespace Application.Calendars.Dtos;

/// <summary>
/// Data Transfer Object representing a user's subscription to a calendar.
/// </summary>
public record CalendarSubscriptionDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarSubscriptionDto"/> record from a <see cref="CalendarSubscription"/> entity.
    /// </summary>
    /// <param name="entity">The source <see cref="CalendarSubscription"/> entity to map from.</param>
    public CalendarSubscriptionDto(CalendarSubscription entity)
    {
        Id = entity.Id;
        UserId = entity.UserId;
        CalendarId = entity.CalendarId;
        IsVisible = entity.IsVisible;
        Calendar = entity.Calendar != null ? new CalendarDto(entity.Calendar) : null;
    }

    /// <summary>
    /// Gets the unique identifier of the subscription.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the identifier of the subscribing user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the identifier of the subscribed calendar.
    /// </summary>
    public long CalendarId { get; init; }

    /// <summary>
    /// Gets a value indicating whether this subscription's calendar is visible to the subscriber.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Gets the details of the subscribed calendar, if loaded.
    /// </summary>
    public CalendarDto? Calendar { get; init; }
}
