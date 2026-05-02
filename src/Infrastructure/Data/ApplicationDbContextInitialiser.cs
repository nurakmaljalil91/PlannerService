using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

/// <summary>
/// Provides methods to initialise and seed the application's database context.
/// In development the database is dropped and recreated on every startup.
/// In all other environments pending EF Core migrations are applied.
/// </summary>
public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContextInitialiser"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging database initialisation events.</param>
    /// <param name="context">The application's database context.</param>
    /// <param name="environment">The host environment used to determine initialisation strategy.</param>
    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        IHostEnvironment environment)
    {
        _logger = logger;
        _context = context;
        _environment = environment;
    }

    /// <summary>
    /// Initialises the database according to the current environment.
    /// In development the database is deleted and recreated from the current model.
    /// In all other environments all pending EF Core migrations are applied.
    /// </summary>
    public async Task InitialiseAsync()
    {
        try
        {
            if (_environment.IsDevelopment())
            {
                // Drop and recreate for a clean dev environment on every startup.
                // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
            }
            else
            {
                // Apply any pending migrations on startup.
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
        }
    }

    /// <summary>
    /// Seeds the application's database with initial data if necessary.
    /// Development-only seed data is applied only in the Development environment.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            if (_environment.IsDevelopment())
            {
                await TrySeedAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    /// <summary>
    /// Attempts to seed the application's database with default data if necessary.
    /// </summary>
    public static Task TrySeedAsync()
    {
        return Task.CompletedTask;
    }
}
