using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        // One-to-many: User -> Projects
        builder.HasMany(u => u.OwnedProjects)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many: User -> CalendarEvents
        builder.HasMany(u => u.CalendarEvents)
            .WithOne(e => e.Owner)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // One-to-many: User -> TeamMemberships
        builder.HasMany(u => u.TeamMemberships)
            .WithOne(tm => tm.User)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // One-to-many: User -> TimerSessions
        builder.HasMany(u => u.TimerSessions)
            .WithOne(ts => ts.User)
            .HasForeignKey(ts => ts.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // // One-to-many: User -> Tags
        // builder.HasMany(u => u.Tags)
        //     .WithOne(t => t.Owner)
        //     .HasForeignKey(t => t.OwnerId)
        //     .OnDelete(DeleteBehavior.Restrict);
        
        // CRITICAL: Friendship multiple FKs (User1 and User2)
        builder.HasMany(u => u.SentFriendships)
            .WithOne(f => f.User1)
            .HasForeignKey(f => f.User1Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.ReceivedFriendships)
            .WithOne(f => f.User2)
            .HasForeignKey(f => f.User2Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        // CRITICAL: ChatMessage multiple FKs (Sender and Recipient)
        builder.HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.Recipient)
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed properties
        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.AvatarInitials);
    }
}