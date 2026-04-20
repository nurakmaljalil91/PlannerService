#nullable enable
using Application.Calendars.Commands.ToggleCalendarVisibility;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.UnitTests.TestInfrastructure;
using Domain.Entities;

namespace Application.UnitTests.Calendars;

/// <summary>
/// Unit tests for <see cref="ToggleCalendarVisibilityCommandHandler"/>.
/// </summary>
public class ToggleCalendarVisibilityCommandHandlerTests
{
    private static IUser MakeUser(Guid userId) => new StubUser(userId);

    /// <summary>
    /// Verifies that toggling visibility flips the IsVisible flag from true to false.
    /// </summary>
    [Fact]
    public async Task Handle_VisibleCalendar_TogglesOff()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        var calendar = new Calendar { Title = "My Cal", IsVisible = true, UserId = userId };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ToggleCalendarVisibilityCommandHandler(context, MakeUser(userId));
        var result = await handler.Handle(new ToggleCalendarVisibilityCommand { CalendarId = calendar.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.False(result.Data!.IsVisible);
    }

    /// <summary>
    /// Verifies that toggling visibility flips the IsVisible flag from false to true.
    /// </summary>
    [Fact]
    public async Task Handle_HiddenCalendar_TogglesOn()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();

        var calendar = new Calendar { Title = "My Cal", IsVisible = false, UserId = userId };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ToggleCalendarVisibilityCommandHandler(context, MakeUser(userId));
        var result = await handler.Handle(new ToggleCalendarVisibilityCommand { CalendarId = calendar.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data!.IsVisible);
    }

    /// <summary>
    /// Verifies that toggling a calendar owned by another user throws <see cref="ForbiddenAccessException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_NotOwner_ThrowsForbiddenAccessException()
    {
        await using var context = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var calendar = new Calendar { Title = "Someone's Cal", IsVisible = true, UserId = ownerId };
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new ToggleCalendarVisibilityCommandHandler(context, MakeUser(otherId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => handler.Handle(new ToggleCalendarVisibilityCommand { CalendarId = calendar.Id }, CancellationToken.None));
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
