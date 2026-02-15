using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(p => p.Color)
            .HasMaxLength(20)
            .IsRequired();

        // One-to-many: Project -> Tasks
        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: Project -> CalendarEvents
        builder.HasMany(p => p.Events)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.CreatedAt);

        // Ignore computed properties
        builder.Ignore(p => p.TasksDone);
        builder.Ignore(p => p.TasksTotal);
        builder.Ignore(p => p.Progress);
    }
}