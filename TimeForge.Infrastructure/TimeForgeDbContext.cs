using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeForge.Models;

namespace TimeForge.Infrastructure;

public class TimeForgeDbContext(DbContextOptions<TimeForgeDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Project> Projects { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<TimeEntry> TimeEntries { get; set; }

    public DbSet<ProjectTask> Tasks { get; set; }

    public DbSet<ProjectTag> ProjectTags { get; set; }

    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TimeForgeDbContext).Assembly);
    }
}