#nullable enable
using Application.Calendars.Commands.UnsubscribeFromCalendar;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Calendars;

/// <summary>
/// Unit tests for <see cref="UnsubscribeFromCalendarCommandHandler"/>.
/// </summary>
public class UnsubscribeFromCalendarCommandHandlerTests
{
    private static IUser MakeUser(Guid userId) => new StubUser(userId);

    /// <summary>
    /// Verifies that unsubscribing from a calendar removes the subscription.
    /// </summary>
    [Fact]
    public async Task Handle_ExistingSubscription_RemovesIt()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        var calendar = new Calendar { Title = "Public Cal", IsPublic = true, UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var subscription = new CalendarSubscription { UserId = userId, CalendarId = calendar.Id };
        context.CalendarSubscriptions.Add(subscription);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UnsubscribeFromCalendarCommandHandler(context, MakeUser(userId));
        var result = await handler.Handle(new UnsubscribeFromCalendarCommand { CalendarId = calendar.Id }, CancellationToken.None);

        Assert.True(result.Success);

        var remaining = context.CalendarSubscriptions.FirstOrDefault(s => s.UserId == userId && s.CalendarId == calendar.Id);
        Assert.Null(remaining);
    }

    /// <summary>
    /// Verifies that unsubscribing when no subscription exists throws <see cref="NotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new UnsubscribeFromCalendarCommandHandler(context, MakeUser(Guid.NewGuid()));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new UnsubscribeFromCalendarCommand { CalendarId = 9999 }, CancellationToken.None));
    }

    private sealed class StubUser : IUser
    {
        private readonly Guid _userId;
        public StubUser(Guid userId) => _userId = userId;
        public string? Username => _userId.ToString();
        public Guid? UserId => _userId;
        public List<string> GetRoles() => [];
    }
}
