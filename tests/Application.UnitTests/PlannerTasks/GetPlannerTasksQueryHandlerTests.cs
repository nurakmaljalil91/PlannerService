using Application.PlannerTasks.Queries.GetPlannerTasks;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;
using Domain.Enums;

namespace Application.UnitTests.PlannerTasks;

/// <summary>
/// Unit tests for <see cref="GetPlannerTasksQueryHandler"/>.
/// </summary>
public class GetPlannerTasksQueryHandlerTests
{
    /// <summary>
    /// Verifies that tasks are returned when their due date or reminder is inside the requested range.
    /// </summary>
    [Fact]
    public async Task Handle_RangeFilter_ReturnsTasksWithDueDateOrReminderInRange()
    {
        await using var context = TestDbContextFactory.Create();
        var calendar = await AddCalendarAsync(context);
        var rangeStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        context.PlannerTasks.AddRange(
            CreateTask("Due In Range", calendar.Id, dueDate: rangeStart.AddDays(1)),
            CreateTask("Reminder In Range", calendar.Id, reminder: rangeStart.AddDays(2)),
            CreateTask("Outside Range", calendar.Id, dueDate: rangeEnd.AddDays(1)),
            CreateTask("Undated", calendar.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPlannerTasksQueryHandler(context);

        var result = await handler.Handle(
            new GetPlannerTasksQuery { RangeStart = rangeStart, RangeEnd = rangeEnd, Total = 10 },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Contains(result.Data.Items!, t => t.Title == "Due In Range");
        Assert.Contains(result.Data.Items!, t => t.Title == "Reminder In Range");
        Assert.DoesNotContain(result.Data.Items!, t => t.Title == "Outside Range");
        Assert.DoesNotContain(result.Data.Items!, t => t.Title == "Undated");
    }

    /// <summary>
    /// Verifies that calendar and range filters are applied together for planner tasks.
    /// </summary>
    [Fact]
    public async Task Handle_RangeFilterWithCalendarId_ReturnsOnlyMatchingCalendarTasks()
    {
        await using var context = TestDbContextFactory.Create();
        var includedCalendar = await AddCalendarAsync(context);
        var excludedCalendar = await AddCalendarAsync(context);
        var rangeStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        context.PlannerTasks.AddRange(
            CreateTask("Included", includedCalendar.Id, dueDate: rangeStart.AddDays(1)),
            CreateTask("Other Calendar", excludedCalendar.Id, dueDate: rangeStart.AddDays(1)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPlannerTasksQueryHandler(context);

        var result = await handler.Handle(
            new GetPlannerTasksQuery
            {
                CalendarId = includedCalendar.Id,
                RangeStart = rangeStart,
                RangeEnd = rangeEnd,
                Total = 10
            },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        var item = Assert.Single(result.Data.Items!);
        Assert.Equal("Included", item.Title);
    }

    /// <summary>
    /// Verifies that existing list behavior is preserved when no range filter is supplied.
    /// </summary>
    [Fact]
    public async Task Handle_NoRangeFilter_ReturnsPaginatedTasks()
    {
        await using var context = TestDbContextFactory.Create();
        var calendar = await AddCalendarAsync(context);

        context.PlannerTasks.AddRange(
            CreateTask("First", calendar.Id),
            CreateTask("Second", calendar.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPlannerTasksQueryHandler(context);

        var result = await handler.Handle(new GetPlannerTasksQuery { Page = 1, Total = 1 }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Single(result.Data.Items!);
    }

    private static async Task<Calendar> AddCalendarAsync(TestApplicationDbContext context)
    {
        var calendar = new Calendar { Title = "Calendar", UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);
        return calendar;
    }

    private static PlannerTask CreateTask(
        string title,
        long calendarId,
        DateTime? dueDate = null,
        DateTime? reminder = null)
        => new()
        {
            Title = title,
            CalendarId = calendarId,
            Priority = PriorityLevel.None,
            DueDate = dueDate,
            Reminder = reminder
        };
}
