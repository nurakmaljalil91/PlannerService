#nullable enable
using Application.Calendars.Commands.CreateCalendar;
using Application.Common.Interfaces;
using Application.UnitTests.TestInfrastructure;

namespace Application.UnitTests.Calendars;

/// <summary>
/// Unit tests for <see cref="CreateCalendarCommandHandler"/>.
/// </summary>
public class CreateCalendarCommandHandlerTests
{
    private static IUser MakeUser(Guid userId) => new StubUser(userId);

    /// <summary>
    /// Verifies that <see cref="CreateCalendarCommandHandler.Handle"/> creates a calendar and returns
    /// a successful response containing the created calendar DTO.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithCalendarDto()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateCalendarCommandHandler(context, MakeUser(Guid.NewGuid()), new StubUserServiceClient());

        var command = new CreateCalendarCommand
        {
            Title = "Work Calendar",
            Description = "Calendar for work events",
            TimeZone = "UTC",
            IsPrimary = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Work Calendar", result.Data.Title);
        Assert.Equal("Calendar for work events", result.Data.Description);
        Assert.True(result.Data.IsPrimary);
    }

    /// <summary>
    /// Verifies that <see cref="CreateCalendarCommandHandler.Handle"/> persists the created calendar
    /// to the database and associates it with the authenticated user.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_PersistsCalendarToDatabase()
    {
        await using var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var handler = new CreateCalendarCommandHandler(context, MakeUser(userId), new StubUserServiceClient());

        var command = new CreateCalendarCommand { Title = "Personal Calendar" };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(result.Data!.Id > 0);

        var saved = await context.Calendars.FindAsync(result.Data.Id);
        Assert.NotNull(saved);
        Assert.Equal("Personal Calendar", saved.Title);
        Assert.Equal(userId, saved.UserId);
    }

    private sealed class StubUser : IUser
    {
        private readonly Guid _userId;

        public StubUser(Guid userId) => _userId = userId;

        public string? Username => _userId.ToString();
        public Guid? UserId => _userId;
        public List<string> GetRoles() => [];
    }

    private sealed class StubUserServiceClient : IUserServiceClient
    {
        public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(true);
    }
}
