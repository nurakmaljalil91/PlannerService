#nullable enable
using System;
using Domain.Entities;

namespace Domain.UnitTests.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Event"/> entity.
/// </summary>
public class EventTests
{
    /// <summary>
    /// Verifies that a new <see cref="Event"/> instance has the expected default values.
    /// </summary>
    [Fact]
    public void Event_Should_Be_Created_With_Default_Values()
    {
        // Arrange
        var newEvent = new Event();

        // Assert
        Assert.NotNull(newEvent);
        Assert.Null(newEvent.Title);
        Assert.NotNull(newEvent.Reminders);
        Assert.Empty(newEvent.Reminders);
        Assert.Equal(DateTime.MinValue, newEvent.StartTime);
    }

    /// <summary>
    /// Verifies that the properties of an <see cref="Event"/> instance can be set correctly.
    /// </summary>
    [Fact]
    public void Event_Properties_Should_Be_Settable()
    {
        // Arrange
        var newEvent = new Event();
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddHours(1);

        // Act
        newEvent.Title = "Test Event";
        newEvent.Description = "Test Description";
        newEvent.StartTime = startTime;
        newEvent.EndTime = endTime;
        newEvent.Location = "Test Location";
        newEvent.IsAllDay = false;
        newEvent.IsRecurring = true;
        newEvent.RecurrenceRule = "RRULE:FREQ=DAILY;COUNT=10";
        newEvent.GoogleEventId = "google-event-id";
        newEvent.CalendarId = 1;
        newEvent.Reminders.Add(new Reminder());

        // Assert
        Assert.Equal("Test Event", newEvent.Title);
        Assert.Equal("Test Description", newEvent.Description);
        Assert.Equal(startTime, newEvent.StartTime);
        Assert.Equal(endTime, newEvent.EndTime);
        Assert.Equal("Test Location", newEvent.Location);
        Assert.False(newEvent.IsAllDay);
        Assert.True(newEvent.IsRecurring);
        Assert.Equal("RRULE:FREQ=DAILY;COUNT=10", newEvent.RecurrenceRule);
        Assert.Equal("google-event-id", newEvent.GoogleEventId);
        Assert.Equal(1, newEvent.CalendarId);
        Assert.Single(newEvent.Reminders);
    }
}
