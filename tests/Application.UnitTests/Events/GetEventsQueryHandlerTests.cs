#nullable enable
using Application.Events.Queries.GetEvents;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Events;

/// <summary>
/// Unit tests for <see cref="GetEventsQueryHandler"/>.
/// </summary>
public class GetEventsQueryHandlerTests
{
    /// <summary>
    /// Verifies that events are returned when their interval overlaps the requested range.
    /// </summary>
    [Fact]
    public async Task Handle_RangeFilter_ReturnsOverlappingEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var calendar = await AddCalendarAsync(context);
        var rangeStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        context.Events.AddRange(
            CreateEvent("Before But Overlaps", calendar.Id, rangeStart.AddHours(-2), rangeStart.AddHours(1)),
            CreateEvent("Inside Range", calendar.Id, rangeStart.AddDays(10), rangeStart.AddDays(10).AddHours(1)),
            CreateEvent("Before Range", calendar.Id, rangeStart.AddDays(-2), rangeStart.AddDays(-1)),
            CreateEvent("At Range End", calendar.Id, rangeEnd, rangeEnd.AddHours(1)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetEventsQueryHandler(context);

        var result = await handler.Handle(
            new GetEventsQuery { RangeStart = rangeStart, RangeEnd = rangeEnd, Total = 10 },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Contains(result.Data.Items!, e => e.Title == "Before But Overlaps");
        Assert.Contains(result.Data.Items!, e => e.Title == "Inside Range");
        Assert.DoesNotContain(result.Data.Items!, e => e.Title == "Before Range");
        Assert.DoesNotContain(result.Data.Items!, e => e.Title == "At Range End");
    }

    /// <summary>
    /// Verifies that calendar and range filters are applied together.
    /// </summary>
    [Fact]
    public async Task Handle_RangeFilterWithCalendarId_ReturnsOnlyMatchingCalendarEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var includedCalendar = await AddCalendarAsync(context);
        var excludedCalendar = await AddCalendarAsync(context);
        var rangeStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        context.Events.AddRange(
            CreateEvent("Included", includedCalendar.Id, rangeStart.AddDays(1), rangeStart.AddDays(1).AddHours(1)),
            CreateEvent("Other Calendar", excludedCalendar.Id, rangeStart.AddDays(1), rangeStart.AddDays(1).AddHours(1)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetEventsQueryHandler(context);

        var result = await handler.Handle(
            new GetEventsQuery
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
    /// Verifies that recurring series are included only when they can still produce visible occurrences.
    /// </summary>
    [Fact]
    public async Task Handle_RangeFilter_ReturnsOnlyPotentiallyActiveRecurringEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var calendar = await AddCalendarAsync(context);
        var rangeStart = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

        context.Events.AddRange(
            CreateEvent(
                "Active Recurring",
                calendar.Id,
                new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                isRecurring: true,
                recurrenceRule: "FREQ=WEEKLY;BYDAY=MO;UNTIL=20260831T000000Z"),
            CreateEvent(
                "Ended Recurring",
                calendar.Id,
                new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                isRecurring: true,
                recurrenceRule: "FREQ=WEEKLY;BYDAY=MO;UNTIL=20260601T000000Z"));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetEventsQueryHandler(context);

        var result = await handler.Handle(
            new GetEventsQuery { RangeStart = rangeStart, RangeEnd = rangeEnd, Total = 10 },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        var item = Assert.Single(result.Data.Items!);
        Assert.Equal("Active Recurring", item.Title);
    }

    /// <summary>
    /// Verifies that existing list behavior is preserved when no range filter is supplied.
    /// </summary>
    [Fact]
    public async Task Handle_NoRangeFilter_ReturnsPaginatedEvents()
    {
        await using var context = TestDbContextFactory.Create();
        var calendar = await AddCalendarAsync(context);

        context.Events.AddRange(
            CreateEvent("First", calendar.Id, new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateEvent("Second", calendar.Id, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetEventsQueryHandler(context);

        var result = await handler.Handle(new GetEventsQuery { Page = 1, Total = 1 }, CancellationToken.None);

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

    private static Event CreateEvent(
        string title,
        long calendarId,
        DateTime startTime,
        DateTime endTime,
        bool isRecurring = false,
        string? recurrenceRule = null)
        => new()
        {
            Title = title,
            CalendarId = calendarId,
            StartTime = startTime,
            EndTime = endTime,
            IsRecurring = isRecurring,
            RecurrenceRule = recurrenceRule
        };
}
