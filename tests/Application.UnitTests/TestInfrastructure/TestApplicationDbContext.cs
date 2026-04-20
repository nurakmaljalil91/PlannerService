using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.TestInfrastructure;

/// <summary>
/// Represents a test implementation of <see cref="IApplicationDbContext"/> using Entity Framework Core for unit testing purposes.
/// </summary>
public sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestApplicationDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the <see cref="DbSet{TodoList}"/> representing the collection of task lists.
    /// </summary>
    public DbSet<TodoList> TodoLists => Set<TodoList>();

    /// <summary>
    /// Gets the <see cref="DbSet{TodoItem}"/> representing the collection of task items.
    /// </summary>
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    /// <summary>
    /// Gets the <see cref="DbSet{Calendar}"/> representing the collection of calendars.
    /// </summary>
    public DbSet<Calendar> Calendars => Set<Calendar>();

    /// <summary>
    /// Gets the <see cref="DbSet{Event}"/> representing the collection of calendar events.
    /// </summary>
    public DbSet<Event> Events => Set<Event>();

    /// <summary>
    /// Gets the <see cref="DbSet{PlannerTask}"/> representing the collection of planner tasks.
    /// </summary>
    public DbSet<PlannerTask> PlannerTasks => Set<PlannerTask>();

    /// <summary>
    /// Gets the <see cref="DbSet{Reminder}"/> representing the collection of reminders.
    /// </summary>
    public DbSet<Reminder> Reminders => Set<Reminder>();

    /// <summary>
    /// Configures the entity mappings for the context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoList>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.OwnsOne(x => x.Colour);
            builder.Navigation(x => x.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<TodoItem>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.List)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ListId);
        });

        modelBuilder.Entity<Calendar>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Navigation(x => x.Events)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.Tasks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Event>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Calendar)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.CalendarId);
            builder.Navigation(x => x.Reminders)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PlannerTask>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Calendar)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.CalendarId);
        });

        modelBuilder.Entity<Reminder>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Event)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.EventId);
        });
    }
}
