using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TimeForge.Models;

namespace TimeForge.Infrastructure.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        //Relationships
        builder.HasMany(p => p.ProjectTags)
            .WithOne(pt => pt.Project)
            .HasForeignKey(pt => pt.ProjectId);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedBy)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.AssignedTo)
            .WithMany(u => u.AssignedProjects)
            .HasForeignKey(p => p.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        //Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.IsDeleted);

        //Filter out soft deleted entities unless explicitly asked for
        builder.HasQueryFilter(p => p.IsDeleted == false);
    }
}