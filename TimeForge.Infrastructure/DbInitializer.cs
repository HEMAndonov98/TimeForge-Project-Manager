using Microsoft.EntityFrameworkCore;
using TimeForge.Infrastructure.Seeders;
using TimeForge.Models;

namespace TimeForge.Infrastructure;

public static class DbInitializer
{
    private const string SEED_USER_ID = "your_stable_seed_user_id_here";

    public static async Task SeedAsync(TimeForgeDbContext context)
    {
        List<Tag> seededTags = new();
        List<Project> seededProjects = new();

        if (!await context.Tags.AnyAsync())
        {
             seededTags = await TagSeeder.Seed(context, SEED_USER_ID);
        }

        if (!await context.Projects.AnyAsync())
        {
            seededProjects = await ProjectSeeder.Seed(context, SEED_USER_ID);
            await ProjectTaskSeeder.Seed(context, seededProjects);
            await ProjectTagSeeder.Seed(context, seededProjects, seededTags);
        }
    }
}