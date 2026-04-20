using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Provides the Entity Framework configuration for the <see cref="CalendarSubscription"/> entity.
/// </summary>
public class CalendarSubscriptionConfiguration : IEntityTypeConfiguration<CalendarSubscription>
{
    /// <summary>
    /// Configures the <see cref="CalendarSubscription"/> entity type for Entity Framework.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<CalendarSubscription> builder)
    {
        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.CalendarId)
            .IsRequired();

        builder.HasOne(s => s.Calendar)
            .WithMany()
            .HasForeignKey(s => s.CalendarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.UserId, s.CalendarId })
            .IsUnique();
    }
}
