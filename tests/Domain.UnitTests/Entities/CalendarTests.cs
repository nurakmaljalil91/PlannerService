#nullable enable
using System;
using Domain.Entities;
using Domain.Enums;

namespace Domain.UnitTests.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Calendar"/> entity.
/// </summary>
public class CalendarTests
{
    /// <summary>
    /// Verifies that a new <see cref="Calendar"/> instance has the expected default values.
    /// </summary>
    [Fact]
    public void Calendar_Should_Be_Created_With_Default_Values()
    {
        // Arrange
        var calendar = new Calendar();

        // Assert
        Assert.NotNull(calendar);
        Assert.Null(calendar.Title);
        Assert.NotNull(calendar.Events);
        Assert.Empty(calendar.Events);
        Assert.NotNull(calendar.Tasks);
        Assert.Empty(calendar.Tasks);
        Assert.Equal(Guid.Empty, calendar.UserId);
    }

    /// <summary>
    /// Verifies that the properties of a <see cref="Calendar"/> instance can be set correctly.
    /// </summary>
    [Fact]
    public void Calendar_Properties_Should_Be_Settable()
    {
        // Arrange
        var calendar = new Calendar();
        var userId = Guid.NewGuid();

        // Act
        calendar.Title = "Test Calendar";
        calendar.Description = "Test Description";
        calendar.TimeZone = "UTC";
        calendar.IsPrimary = true;
        calendar.IsGoogleCalendar = false;
        calendar.GoogleCalendarId = "google-id";
        calendar.UserId = userId;
        calendar.Events.Add(new Event());
        calendar.Tasks.Add(new PlannerTask());

        // Assert
        Assert.Equal("Test Calendar", calendar.Title);
        Assert.Equal("Test Description", calendar.Description);
        Assert.Equal("UTC", calendar.TimeZone);
        Assert.True(calendar.IsPrimary);
        Assert.False(calendar.IsGoogleCalendar);
        Assert.Equal("google-id", calendar.GoogleCalendarId);
        Assert.Equal(userId, calendar.UserId);
        Assert.Single(calendar.Events);
        Assert.Single(calendar.Tasks);
    }
}
