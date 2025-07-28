using Microsoft.EntityFrameworkCore;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Seeders;

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
                Id = Guid.NewGuid().ToString(),
                Name = $"Project Nebula {i}",
                IsPublic = i % 3 == 0, // Some public, some private
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30 * (i % 5)).AddDays(random.Next(0, 15))), 
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-i * 5).AddHours(random.Next(0, 24))
            };
            projects.Add(project);
        }

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
        return projects;
    }
}