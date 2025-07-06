using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Configure relationships
        builder.HasMany(u => u.Projects)
            .WithOne(p => p.CreatedBy)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Tags)
            .WithOne(t => t.CreatedBy)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.TimeEntries)
            .WithOne(te => te.CreatedBy)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}