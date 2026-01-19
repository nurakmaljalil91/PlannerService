#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebAPI.Services;

namespace WebAPI.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="CurrentUser"/>.
/// </summary>
public class CurrentUserTests
{
    /// <summary>
    /// Verifies that the username is correctly retrieved from the NameIdentifier claim.
    /// </summary>
    [Fact]
    public void Username_ReturnsNameIdentifierClaim()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "alice")
            }))
        };

        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var currentUser = new CurrentUser(accessor);

        Assert.Equal("alice", currentUser.Username);
    }

    /// <summary>
    /// Verifies that roles are correctly retrieved from the Role claims.
    /// </summary>
    [Fact]
    public void GetRoles_ReturnsRoleClaims()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Support")
            }))
        };

        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var currentUser = new CurrentUser(accessor);

        var roles = currentUser.GetRoles();

        Assert.Contains("Admin", roles);
        Assert.Contains("Support", roles);
    }

    /// <summary>
    /// Verifies that an empty list of roles is returned when there is no HTTP context.
    /// </summary>
    [Fact]
    public void GetRoles_ReturnsEmptyWhenNoContext()
    {
        var accessor = new HttpContextAccessor();
        var currentUser = new CurrentUser(accessor);

        var roles = currentUser.GetRoles();

        Assert.Empty(roles);
    }
}
