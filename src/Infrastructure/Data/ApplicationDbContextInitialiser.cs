using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

/// <summary>
/// Provides methods to initialise and seed the application's database context.
/// </summary>
public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContextInitialiser"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging database initialisation events.</param>
    /// <param name="context">The application's database context.</param>
    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Ensures that the application's database is deleted and created anew.
    /// </summary>
    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
        }
    }

    /// <summary>
    /// Seeds the application's database with initial data if necessary.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
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
