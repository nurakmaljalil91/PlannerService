#nullable enable

namespace Application.Common.Interfaces;

/// <summary>
/// Provides access to user data owned by UserService.
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Determines whether the specified user exists in UserService.
    /// </summary>
    /// <param name="userId">The identifier of the user to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> when the user exists; otherwise, <c>false</c>.</returns>
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
}
