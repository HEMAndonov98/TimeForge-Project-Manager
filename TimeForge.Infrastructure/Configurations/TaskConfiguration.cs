using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = TimeForge.Models.Task;

namespace TimeForge.Infrastructure.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        // Configure relationship with Project
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationship with TimeEntries
        builder.HasMany(t => t.TimeEntries)
            .WithOne(te => te.Task)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.ProjectId, t.IsCompleted });
    }
}