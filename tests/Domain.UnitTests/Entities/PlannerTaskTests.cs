#nullable enable
using System;
using Domain.Entities;
using Domain.Enums;

namespace Domain.UnitTests.Entities;

/// <summary>
/// Contains unit tests for the <see cref="PlannerTask"/> entity.
/// </summary>
public class PlannerTaskTests
{
    /// <summary>
    /// Verifies that a new <see cref="PlannerTask"/> instance has the expected default values.
    /// </summary>
    [Fact]
    public void PlannerTask_Should_Be_Created_With_Default_Values()
    {
        // Arrange
        var task = new PlannerTask();

        // Assert
        Assert.NotNull(task);
        Assert.Null(task.Title);
        Assert.False(task.IsCompleted);
        Assert.Equal(PriorityLevel.None, task.Priority);
    }

    /// <summary>
    /// Verifies that the properties of a <see cref="PlannerTask"/> instance can be set correctly.
    /// </summary>
    [Fact]
    public void PlannerTask_Properties_Should_Be_Settable()
    {
        // Arrange
        var task = new PlannerTask();
        var dueDate = DateTime.UtcNow.AddDays(1);
        var reminderDate = dueDate.AddHours(-1);

        // Act
        task.Title = "Test Task";
        task.Note = "Test Note";
        task.Priority = PriorityLevel.High;
        task.DueDate = dueDate;
        task.Reminder = reminderDate;
        task.IsCompleted = true;
        task.CalendarId = 1;

        // Assert
        Assert.Equal("Test Task", task.Title);
        Assert.Equal("Test Note", task.Note);
        Assert.Equal(PriorityLevel.High, task.Priority);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(reminderDate, task.Reminder);
        Assert.True(task.IsCompleted);
        Assert.Equal(1, task.CalendarId);
    }
}
