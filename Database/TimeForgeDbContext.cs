using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using TimeForge.Models;
using TimeForge.Models.Common;

namespace TimeForge.Database;

public class TimeForgeDbContext(DbContextOptions<TimeForgeDbContext> options) : IdentityDbContext<User>(options)
{

    public DbSet<Project> Projects { get; set; }
    public DbSet<TimerSession> TimerSessions { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(TimeForgeDbContext).Assembly);
    }

    private static void ConfigureGlobalIds(ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // Skip owned / query / identity framework internals
            if (entity.IsOwned())
                continue;

            var clrType = entity.ClrType;
            if (clrType == null)
                continue;

            // Only apply to your base model hierarchy
            if (!InheritsFromBaseModel(clrType))
                continue;

            var idProp = entity.FindProperty("Id");
            if (idProp == null)
                continue;

            var type = idProp.ClrType;

            if (type == typeof(int))
            {
                // Database identity
                idProp.ValueGenerated = ValueGenerated.OnAdd;
            }
            else if (type == typeof(Guid) || type == typeof(string))
            {
                // Client-side generation in BaseModel ctor
                idProp.ValueGenerated = ValueGenerated.Never;
            }
        }
    }

    private static bool InheritsFromBaseModel(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(BaseModel<>))
                return true;

            type = type.BaseType!;
        }

        return false;
    }
}