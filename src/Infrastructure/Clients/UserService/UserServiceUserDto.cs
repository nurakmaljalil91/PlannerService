#nullable enable

namespace Infrastructure.Clients.UserService;

/// <summary>
/// Represents a user returned by UserService.
/// </summary>
internal sealed class UserServiceUserDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    public string? Email { get; init; }
}
