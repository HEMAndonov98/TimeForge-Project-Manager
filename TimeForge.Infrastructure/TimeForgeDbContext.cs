using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeForge.Models;
using Task = System.Threading.Tasks.Task;

namespace TimeForge.Infrastructure;

public class TimeForgeDbContext(DbContextOptions<TimeForgeDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Project> Projects { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<TimeEntry> TimeEntries { get; set; }

    public DbSet<Task> Tasks { get; set; }
}