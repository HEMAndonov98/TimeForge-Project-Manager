using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class TaskCollectionConfiguration : IEntityTypeConfiguration<TaskCollection>
{
    public void Configure(EntityTypeBuilder<TaskCollection> builder)
    {
        builder.HasOne(tc => tc.User)
            .WithMany(u => u.TaskCollections)
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(tc => tc.TaskItems)
            .WithOne(ti => ti.TaskCollection)
            .HasForeignKey(ti => ti.TaskCollectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}