#nullable enable

namespace Infrastructure.Clients.UserService;

/// <summary>
/// Represents the standard UserService response envelope.
/// </summary>
/// <typeparam name="T">The response data type.</typeparam>
internal sealed class UserServiceResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the UserService operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the UserService response message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the UserService response data.
    /// </summary>
    public T? Data { get; init; }
}
