#nullable enable
using Application.Common.Interfaces;

namespace Application.UnitTests.TestInfrastructure;

/// <summary>
/// A fixed <see cref="IUser"/> for handler tests.
/// </summary>
public sealed class TestUser : IUser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestUser"/> class.
    /// </summary>
    /// <param name="userId">The user identifier, or <c>null</c> for an unidentified caller.</param>
    /// <param name="roles">The roles held by the user.</param>
    public TestUser(Guid? userId, params string[] roles)
    {
        UserId = userId;
        Roles = roles.ToList();
    }

    /// <summary>
    /// Gets a new user with a random identifier.
    /// </summary>
    public static TestUser Random() => new(Guid.NewGuid());

    /// <inheritdoc />
    public string? Username => UserId?.ToString();

    /// <inheritdoc />
    public Guid? UserId { get; }

    private List<string> Roles { get; }

    /// <inheritdoc />
    public List<string> GetRoles() => Roles;
}
