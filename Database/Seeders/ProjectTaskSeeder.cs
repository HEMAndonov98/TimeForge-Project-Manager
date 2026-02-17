using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Database.Seeders;

public static class ProjectTaskSeeder
{
    public static async Task<List<ProjectTask>> Seed(TimeForgeDbContext context, List<Project> projects)
    {
        var projectTasks = new List<ProjectTask>();
        var random = new Random();

        foreach (var project in projects)
        {
            for (int j = 1; j <= 3 + (project.Name.Length % 3); j++) 
            {
                var task = new ProjectTask
                {
                };
                projectTasks.Add(task);
            }
        }

        await context.Tasks.AddRangeAsync(projectTasks);
        await context.SaveChangesAsync();
        return projectTasks;
    }
}