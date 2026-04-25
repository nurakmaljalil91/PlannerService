#nullable enable

namespace Application.Common.Interfaces;

/// <summary>
/// Provides the current HTTP bearer authorization header for downstream service calls.
/// </summary>
public interface ICurrentBearerTokenProvider
{
    /// <summary>
    /// Gets the current authorization header value, or <c>null</c> when no bearer token is available.
    /// </summary>
    string? AuthorizationHeader { get; }
}
