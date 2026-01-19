#nullable enable
using System;
using Domain.Entities;

namespace Domain.UnitTests.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Reminder"/> entity.
/// </summary>
public class ReminderTests
{
    /// <summary>
    /// Verifies that a new <see cref="Reminder"/> instance has the expected default values.
    /// </summary>
    [Fact]
    public void Reminder_Should_Be_Created_With_Default_Values()
    {
        // Arrange
        var reminder = new Reminder();

        // Assert
        Assert.NotNull(reminder);
        Assert.Null(reminder.Title);
        Assert.False(reminder.IsSent);
        Assert.Equal(DateTime.MinValue, reminder.ReminderDateTime);
    }

    /// <summary>
    /// Verifies that the properties of a <see cref="Reminder"/> instance can be set correctly.
    /// </summary>
    [Fact]
    public void Reminder_Properties_Should_Be_Settable()
    {
        // Arrange
        var reminder = new Reminder();
        var reminderDateTime = DateTime.UtcNow.AddMinutes(30);

        // Act
        reminder.Title = "Test Reminder";
        reminder.ReminderDateTime = reminderDateTime;
        reminder.IsSent = true;
        reminder.EventId = 1;

        // Assert
        Assert.Equal("Test Reminder", reminder.Title);
        Assert.Equal(reminderDateTime, reminder.ReminderDateTime);
        Assert.True(reminder.IsSent);
        Assert.Equal(1, reminder.EventId);
    }
}
