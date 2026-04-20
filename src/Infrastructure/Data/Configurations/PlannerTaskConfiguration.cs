using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Provides the Entity Framework configuration for the <see cref="PlannerTask"/> entity.
/// </summary>
public class PlannerTaskConfiguration : IEntityTypeConfiguration<PlannerTask>
{
    /// <summary>
    /// Configures the <see cref="PlannerTask"/> entity type for Entity Framework.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PlannerTask> builder)
    {
        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Note)
            .HasMaxLength(1000);

        builder.Property(t => t.Priority)
            .IsRequired();

        builder.Property(t => t.CalendarId)
            .IsRequired();

        builder.HasOne(t => t.Calendar)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CalendarId);
    }
}
