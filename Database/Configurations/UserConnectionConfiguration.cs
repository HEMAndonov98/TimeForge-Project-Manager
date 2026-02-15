using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class UserConnectionConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasKey(uc => new { uc.FromUserId, uc.ToUserId });

        builder.HasOne(uc => uc.FromUser)
            .WithMany(u => u.SentConnections)
            .HasForeignKey(uc => uc.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        builder.HasOne(uc => uc.ToUser)
            .WithMany(u => u.ReceivedConnections)
            .HasForeignKey(uc => uc.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}