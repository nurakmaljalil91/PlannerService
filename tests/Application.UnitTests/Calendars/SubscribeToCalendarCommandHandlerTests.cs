#nullable enable
using Application.Calendars.Commands.SubscribeToCalendar;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Calendars;

/// <summary>
/// Unit tests for <see cref="SubscribeToCalendarCommandHandler"/>.
/// </summary>
public class SubscribeToCalendarCommandHandlerTests
{
    private static IUser MakeUser(Guid userId) => new StubUser(userId);

    /// <summary>
    /// Verifies that subscribing to a public calendar succeeds and returns the subscription DTO.
    /// </summary>
    [Fact]
    public async Task Handle_PublicCalendar_CreatesSubscription()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        var calendar = new Calendar { Title = "Public Cal", IsPublic = true, UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new SubscribeToCalendarCommandHandler(context, MakeUser(userId));
        var result = await handler.Handle(new SubscribeToCalendarCommand { CalendarId = calendar.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(calendar.Id, result.Data.CalendarId);
        Assert.True(result.Data.IsVisible);
    }

    /// <summary>
    /// Verifies that subscribing twice to the same calendar throws <see cref="ConflictException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_AlreadySubscribed_ThrowsConflictException()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        var calendar = new Calendar { Title = "Public Cal", IsPublic = true, UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var subscription = new CalendarSubscription { UserId = userId, CalendarId = calendar.Id };
        context.CalendarSubscriptions.Add(subscription);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new SubscribeToCalendarCommandHandler(context, MakeUser(userId));

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new SubscribeToCalendarCommand { CalendarId = calendar.Id }, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that subscribing to a private calendar throws <see cref="ForbiddenAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_PrivateCalendar_ThrowsForbiddenAccessException()
    {
        await using var context = TestDbContextFactory.Create();

        var calendar = new Calendar { Title = "Private Cal", IsPublic = false, UserId = Guid.NewGuid() };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new SubscribeToCalendarCommandHandler(context, MakeUser(Guid.NewGuid()));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => handler.Handle(new SubscribeToCalendarCommand { CalendarId = calendar.Id }, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that subscribing to a non-existent calendar throws <see cref="NotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_CalendarNotFound_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SubscribeToCalendarCommandHandler(context, MakeUser(Guid.NewGuid()));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SubscribeToCalendarCommand { CalendarId = 9999 }, CancellationToken.None));
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
