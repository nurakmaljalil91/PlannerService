#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace IntegrationTests;

/// <summary>
/// Base class for API integration tests.
/// </summary>
public abstract class ApiTestBase
{
    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiTestBase"/> class.
    /// </summary>
    /// <param name="factory">The API factory.</param>
    protected ApiTestBase(ApiFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Creates a new HTTP client.
    /// </summary>
    /// <returns>An <see cref="HttpClient"/> instance.</returns>
    protected HttpClient CreateClient()
        => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

    /// <summary>
    /// Creates a new authenticated HTTP client.
    /// </summary>
    /// <returns>An authenticated <see cref="HttpClient"/> instance.</returns>
    protected HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        var token = CreateToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Reads the response content and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The deserialized response content.</returns>
    protected static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        Assert.NotNull(payload);
        return payload!;
    }

    private static string CreateToken()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "integration-user"),
            new(ClaimTypes.Email, "integration@example.com"),
            new(ClaimTypes.Role, "Admin")
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.JwtKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(ApiFactory.JwtExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: ApiFactory.JwtIssuer,
            audience: ApiFactory.JwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Represents a paginated response.
    /// </summary>
    /// <typeparam name="T">The type of the items in the response.</typeparam>
    protected sealed class PaginatedResponse<T>
    {
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public IEnumerable<T>? Items { get; set; }
        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// Gets or sets the total count of items.
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Represents a task item response.
    /// </summary>
    protected sealed class TodoItemResponse
    {
        /// <summary>
        /// Gets or sets the ID of the task item.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the ID of the list the task item belongs to.
        /// </summary>
        public long ListId { get; set; }
        /// <summary>
        /// Gets or sets the title of the task item.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the task item is done.
        /// </summary>
        public bool Done { get; set; }
    }

    /// <summary>
    /// Represents a task list response.
    /// </summary>
    protected sealed class TodoListResponse
    {
        /// <summary>
        /// Gets or sets the ID of the task list.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the title of the task list.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Gets or sets the colour of the task list.
        /// </summary>
        public string? Colour { get; set; }
        /// <summary>
        /// Gets or sets the items in the task list.
        /// </summary>
        public IReadOnlyCollection<TodoItemResponse>? Items { get; set; }
    }
}
