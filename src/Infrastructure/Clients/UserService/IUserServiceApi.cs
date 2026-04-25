#nullable enable
using Refit;

namespace Infrastructure.Clients.UserService;

/// <summary>
/// Refit API contract for UserService user endpoints.
/// </summary>
internal interface IUserServiceApi
{
    /// <summary>
    /// Gets a user by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the user.</param>
    /// <param name="authorizationHeader">The bearer authorization header to forward.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The UserService API response.</returns>
    [Get("/api/users/{id}")]
    Task<ApiResponse<UserServiceResponse<UserServiceUserDto>>> GetUserByIdAsync(
        Guid id,
        [Header("Authorization")] string? authorizationHeader,
        CancellationToken cancellationToken);
}
