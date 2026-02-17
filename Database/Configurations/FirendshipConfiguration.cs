using Microsoft.EntityFrameworkCore;
using TimeForge.Models;

namespace TimeForge.Database.Configurations;

public class FirendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Friendship> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(f => f.User1Id)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(f => f.User2Id)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Indexes
        builder.HasIndex(f => f.User1Id);
        builder.HasIndex(f => f.User2Id);

        // Unique constraint
        builder.HasIndex(f => new { f.User1Id, f.User2Id })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
