using TimeForge.Models;

namespace TimeForge.Infrastructure.Seeders;

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
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Task {j} for {project.Name.Replace("Project Nebula ", "P")}",
                    IsBillable = random.Next(0, 2) == 0, // 50/50 billable
                    IsCompleted = random.Next(0, 3) == 0, // ~33% completed
                    CompletionDate = random.Next(0, 3) == 0 ? DateTime.UtcNow.AddDays(-random.Next(5, 30)) : (DateTime?)null,
                    ProjectId = project.Id,
                    CreatedAt = project.CreatedAt.AddHours(random.Next(1, 48))
                };
                projectTasks.Add(task);
            }
        }

        await context.Tasks.AddRangeAsync(projectTasks);
        await context.SaveChangesAsync();
        return projectTasks;
    }
}