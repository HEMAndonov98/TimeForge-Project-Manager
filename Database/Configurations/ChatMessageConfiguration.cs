using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace Database.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(m => m.SenderId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(m => m.Content)
            .HasMaxLength(5000)
            .IsRequired();

        // Indexes for conversation queries
        builder.HasIndex(m => new { m.SenderId, m.RecipientId });

        // CHECK constraint added in migration:
        // Either RecipientId OR TeamId must be set, not both
    }
}
