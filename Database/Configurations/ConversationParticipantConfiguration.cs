using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.HasKey(cp => cp.Id); // Uses string ID because it inherits from BaseDeletableModel<string>
        
        // Define relationship with Conversation
        builder.HasOne(cp => cp.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Define relationship with User
        builder.HasOne(cp => cp.User)
            .WithMany(u => u.ConversationParticipations)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(cp => new { cp.ConversationId, cp.UserId }).IsUnique();
    }
}
