using Microsoft.EntityFrameworkCore;

using TimeForge.Infrastructure.Seeders;
using TimeForge.Models;

namespace TimeForge.Infrastructure;

public static class DbInitializer
{
    //TODO Testing purposes delete in production
    private const string SEED_USER_ID = "42859591-f3e5-4c80-9f28-3310bafe4c3e";

    public static async Task SeedAsync(TimeForgeDbContext context)
    {
        List<Project> seededProjects = new();

        if (!await context.Projects.AnyAsync())
        {
            seededProjects = await ProjectSeeder.Seed(context, SEED_USER_ID);
            await ProjectTaskSeeder.Seed(context, seededProjects);
        }
    }
}