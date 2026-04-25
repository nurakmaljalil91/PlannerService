#nullable enable
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests;

/// <summary>
/// A web application factory for integration tests.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// The JWT issuer used by integration tests.
    /// </summary>
    public const string JwtIssuer = "IntegrationTests";

    /// <summary>
    /// The JWT audience used by integration tests.
    /// </summary>
    public const string JwtAudience = "IntegrationTests";

    /// <summary>
    /// The JWT signing key used by integration tests.
    /// </summary>
    public const string JwtKey = "integration-tests-super-secret-key-1234567890";

    /// <summary>
    /// The JWT expiry in minutes used by integration tests.
    /// </summary>
    public const int JwtExpiryMinutes = 60;

    /// <summary>
    /// Configures the web host for integration tests.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true",
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:Key"] = JwtKey,
                ["Jwt:ExpiryMinutes"] = JwtExpiryMinutes.ToString(),
                ["BuildVersion"] = "integration-test-build"
            };

            config.AddInMemoryCollection(settings);
        });
    }
}
