#nullable enable
using Application.Calendars.Commands.CreateCalendar;
using Application.Calendars.Commands.DeleteCalendar;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.UnitTests.TestInfrastructure;

namespace Application.UnitTests.Calendars;

/// <summary>
/// Unit tests for <see cref="DeleteCalendarCommandHandler"/>.
/// </summary>
public class DeleteCalendarCommandHandlerTests
{
    private static IUser MakeUser() => new StubUser(Guid.NewGuid());

    /// <summary>
    /// Verifies that <see cref="DeleteCalendarCommandHandler.Handle"/> throws a <see cref="NotFoundException"/>
    /// when attempting to delete a calendar that does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCalendarMissing_ThrowsNotFoundException()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new DeleteCalendarCommandHandler(context);

        var command = new DeleteCalendarCommand { Id = 999 };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <see cref="DeleteCalendarCommandHandler.Handle"/> successfully deletes
    /// an existing calendar and returns a successful response.
    /// </summary>
    [Fact]
    public async Task Handle_ExistingCalendar_DeletesAndReturnsSuccess()
    {
        await using var context = TestDbContextFactory.Create();

        var createHandler = new CreateCalendarCommandHandler(context, MakeUser());
        var created = await createHandler.Handle(new CreateCalendarCommand { Title = "To Delete" }, CancellationToken.None);

        var deleteHandler = new DeleteCalendarCommandHandler(context);
        var result = await deleteHandler.Handle(
            new DeleteCalendarCommand { Id = created.Data!.Id },
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(await context.Calendars.FindAsync(created.Data.Id));
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
