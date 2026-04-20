using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Provides the Entity Framework configuration for the <see cref="Event"/> entity.
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    /// <summary>
    /// Configures the <see cref="Event"/> entity type for Entity Framework.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Location)
            .HasMaxLength(500);

        builder.Property(e => e.RecurrenceRule)
            .HasMaxLength(500);

        builder.Property(e => e.GoogleEventId)
            .HasMaxLength(500);

        builder.Property(e => e.CalendarId)
            .IsRequired();

        builder.HasOne(e => e.Calendar)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CalendarId);

        builder.Navigation(e => e.Reminders)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
