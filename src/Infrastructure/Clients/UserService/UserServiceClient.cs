#nullable enable
using System.Net;
using Application.Common.Interfaces;

namespace Infrastructure.Clients.UserService;

/// <summary>
/// UserService client implementation backed by Refit.
/// </summary>
internal sealed class UserServiceClient : IUserServiceClient
{
    private readonly IUserServiceApi _api;
    private readonly ICurrentBearerTokenProvider _bearerTokenProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceClient"/> class.
    /// </summary>
    /// <param name="api">The Refit UserService API.</param>
    /// <param name="bearerTokenProvider">The current bearer token provider.</param>
    public UserServiceClient(IUserServiceApi api, ICurrentBearerTokenProvider bearerTokenProvider)
    {
        _api = api;
        _bearerTokenProvider = bearerTokenProvider;
    }

    /// <inheritdoc />
    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var response = await _api.GetUserByIdAsync(userId, _bearerTokenProvider.AuthorizationHeader, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"UserService returned {(int)response.StatusCode} while checking user {userId}.");
        }

        return response.Content?.Success == true && response.Content.Data?.Id == userId;
    }
}
