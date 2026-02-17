using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Database.Seeders;

public static class ProjectSeeder
{ 
    public static async Task<List<Project>> Seed(TimeForgeDbContext context, string userId)
    {
        var projects = new List<Project>();
        var random = new Random();

        for (int i = 1; i <= 25; i++) 
        {
            var project = new Project
            {
            };
            projects.Add(project);
        }

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
        return projects;
    }
}