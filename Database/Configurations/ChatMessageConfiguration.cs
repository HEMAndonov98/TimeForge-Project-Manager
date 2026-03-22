using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Database.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(m => m.ConversationId)
            .HasMaxLength(36)
            .IsRequired();

        // Indexes for conversation queries
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.SenderId);
    }
}
