using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Provides the Entity Framework configuration for the <see cref="Calendar"/> entity.
/// </summary>
public class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    /// <summary>
    /// Configures the <see cref="Calendar"/> entity type for Entity Framework.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.TimeZone)
            .HasMaxLength(100);

        builder.Property(c => c.GoogleCalendarId)
            .HasMaxLength(500);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.IsPublic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Color)
            .HasMaxLength(20);

        builder.Navigation(c => c.Events)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
