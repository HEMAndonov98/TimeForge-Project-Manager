using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class ProjectTagConfiguration : IEntityTypeConfiguration<ProjectTag>
{
    public void Configure(EntityTypeBuilder<ProjectTag> builder)
    {
        builder.HasKey(pt => new { pt.ProjectId, pt.TagId });

        builder.HasOne(pt => pt.Project)
            .WithMany(p => p.ProjectTags)
            .HasForeignKey(pt => pt.ProjectId);

        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.ProjectTags)
            .HasForeignKey(pt => pt.TagId);
        
        builder.HasIndex(pt => new { pt.ProjectId, pt.TagId})
            .IsUnique();
        
        
    }
}