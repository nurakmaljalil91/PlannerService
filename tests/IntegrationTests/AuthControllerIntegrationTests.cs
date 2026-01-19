#nullable enable
using System.Net;
using System.Net.Http.Json;
using Domain.Common;

namespace IntegrationTests;

/// <summary>
/// Integration tests for the <see cref="WebAPI.Controllers.AuthController"/>.
/// </summary>
[Collection("Integration")]
public class AuthControllerIntegrationTests : ApiTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthControllerIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The API factory.</param>
    public AuthControllerIntegrationTests(ApiFactory factory)
        : base(factory)
    {
    }

    /// <summary>
    /// Verifies that the login endpoint returns a token.
    /// </summary>
    [Fact]
    public async Task Login_ReturnsToken()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            Username = "user",
            Email = "user@example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await ReadResponseAsync<BaseResponse<AuthTokenResponse>>(response);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Data);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data!.Token));
        Assert.True(payload.Data!.ExpiresAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }
}
