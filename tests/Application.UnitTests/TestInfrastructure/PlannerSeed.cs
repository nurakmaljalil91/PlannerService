#nullable enable
using Domain.Entities;

namespace Application.UnitTests.TestInfrastructure;

/// <summary>
/// Seeds calendars, events, planner tasks and reminders for ownership tests.
/// </summary>
public static class PlannerSeed
{
    /// <summary>
    /// Adds a calendar owned by <paramref name="ownerId"/>.
    /// </summary>
    public static async Task<Calendar> CalendarAsync(TestApplicationDbContext context, Guid? ownerId, bool isPublic = false)
    {
        var calendar = new Calendar { Title = isPublic ? "Public" : "Private", UserId = ownerId ?? Guid.Empty, IsPublic = isPublic };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);
        return calendar;
    }

    /// <summary>
    /// Adds an event to <paramref name="calendar"/>.
    /// </summary>
    public static async Task<Event> EventAsync(TestApplicationDbContext context, Calendar calendar, string title = "Event")
    {
        var start = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc);
        var entity = new Event { Title = title, StartTime = start, EndTime = start.AddHours(1), CalendarId = calendar.Id };
        context.Events.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    /// <summary>
    /// Adds a planner task to <paramref name="calendar"/>.
    /// </summary>
    public static async Task<PlannerTask> TaskAsync(TestApplicationDbContext context, Calendar calendar, string title = "Task")
    {
        var entity = new PlannerTask { Title = title, CalendarId = calendar.Id };
        context.PlannerTasks.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    /// <summary>
    /// Adds a reminder to <paramref name="parent"/>.
    /// </summary>
    public static async Task<Reminder> ReminderAsync(TestApplicationDbContext context, Event parent, string title = "Reminder")
    {
        var entity = new Reminder { Title = title, ReminderDateTime = parent.StartTime.AddMinutes(-15), EventId = parent.Id };
        context.Reminders.Add(entity);
        await context.SaveChangesAsync(CancellationToken.None);
        return entity;
    }

    /// <summary>
    /// Subscribes <paramref name="userId"/> to <paramref name="calendarId"/>.
    /// </summary>
    public static async Task SubscribeAsync(TestApplicationDbContext context, Guid userId, long calendarId)
    {
        context.CalendarSubscriptions.Add(new CalendarSubscription { UserId = userId, CalendarId = calendarId });
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
