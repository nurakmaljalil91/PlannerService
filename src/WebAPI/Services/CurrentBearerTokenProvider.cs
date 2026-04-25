#nullable enable
using Application.Common.Interfaces;
using Microsoft.Net.Http.Headers;

namespace WebAPI.Services;

/// <summary>
/// Provides the current request bearer authorization header for downstream service calls.
/// </summary>
public sealed class CurrentBearerTokenProvider : ICurrentBearerTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentBearerTokenProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The current HTTP context accessor.</param>
    public CurrentBearerTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? AuthorizationHeader
        => _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
}
