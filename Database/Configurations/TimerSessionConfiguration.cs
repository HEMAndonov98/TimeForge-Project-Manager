using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Database.Configurations;

public class TimerSessionConfiguration : IEntityTypeConfiguration<TimerSession>
{
    public void Configure(EntityTypeBuilder<TimerSession> builder)
    {
        // Relationships
        builder
            .HasOne(te => te.Task)
            .WithMany(t => t.TimerSessions)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(te => te.User)
            .WithMany(u => u.TimerSessions)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(ts => ts.IsActive);
        
        builder.HasIndex(te => new { te.UserId })
            .IsUnique()
            .HasFilter("[EndTime] IS NULL");
    }
}