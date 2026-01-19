#nullable enable
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Common;
using Microsoft.AspNetCore.Mvc.Testing;

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
    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var token = await GetTokenAsync(client);
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

    private static async Task<string> GetTokenAsync(HttpClient client)
    {
        var login = new
        {
            Username = "integration-user",
            Email = "integration@example.com"
        };

        var response = await client.PostAsJsonAsync("/api/Auth/login", login);
        response.EnsureSuccessStatusCode();

        var payload = await ReadResponseAsync<BaseResponse<AuthTokenResponse>>(response);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Data);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data!.Token));

        return payload.Data!.Token;
    }

    /// <summary>
    /// Represents the response containing an authentication token.
    /// </summary>
    /// <param name="Token">The JWT token.</param>
    /// <param name="ExpiresAtUtc">The token expiration date and time in UTC.</param>
    protected sealed record AuthTokenResponse(string Token, DateTime ExpiresAtUtc);

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
    /// Represents a todo item response.
    /// </summary>
    protected sealed class TodoItemResponse
    {
        /// <summary>
        /// Gets or sets the ID of the todo item.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the ID of the list the todo item belongs to.
        /// </summary>
        public long ListId { get; set; }
        /// <summary>
        /// Gets or sets the title of the todo item.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the todo item is done.
        /// </summary>
        public bool Done { get; set; }
    }

    /// <summary>
    /// Represents a todo list response.
    /// </summary>
    protected sealed class TodoListResponse
    {
        /// <summary>
        /// Gets or sets the ID of the todo list.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the title of the todo list.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Gets or sets the colour of the todo list.
        /// </summary>
        public string? Colour { get; set; }
        /// <summary>
        /// Gets or sets the items in the todo list.
        /// </summary>
        public IReadOnlyCollection<TodoItemResponse>? Items { get; set; }
    }
}
