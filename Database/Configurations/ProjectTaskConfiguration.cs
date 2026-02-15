using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(t => t.ProjectId)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(t => t.TaskName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // One-to-many: Task -> TimerSessions
        builder.HasMany(t => t.TimerSessions)
            .WithOne(ts => ts.Task)
            .HasForeignKey(ts => ts.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.ProjectId);

        // Ignore computed properties
        builder.Ignore(t => t.TotalTimeSpent);
    }
}