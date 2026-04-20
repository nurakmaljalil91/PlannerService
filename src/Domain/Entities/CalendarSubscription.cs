#nullable enable
using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Represents a user's subscription to a public calendar.
/// </summary>
public class CalendarSubscription : BaseAuditableEntity
{
    /// <summary>
    /// Gets or sets the identifier of the subscribing user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the subscribed calendar.
    /// </summary>
    public long CalendarId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this subscription's calendar is visible to the subscriber.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the subscribed calendar.
    /// </summary>
    public Calendar? Calendar { get; set; }
}
