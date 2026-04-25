using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Represents the application's database context, providing access to all entity sets.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets the set of <see cref="Calendar"/> entities.
    /// </summary>
    DbSet<Calendar> Calendars { get; }

    /// <summary>
    /// Gets the set of <see cref="Event"/> entities.
    /// </summary>
    DbSet<Event> Events { get; }

    /// <summary>
    /// Gets the set of <see cref="PlannerTask"/> entities.
    /// </summary>
    DbSet<PlannerTask> PlannerTasks { get; }

    /// <summary>
    /// Gets the set of <see cref="Reminder"/> entities.
    /// </summary>
    DbSet<Reminder> Reminders { get; }

    /// <summary>
    /// Gets the set of <see cref="CalendarSubscription"/> entities.
    /// </summary>
    DbSet<CalendarSubscription> CalendarSubscriptions { get; }

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
