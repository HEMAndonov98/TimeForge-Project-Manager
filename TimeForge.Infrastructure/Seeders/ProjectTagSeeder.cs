using Microsoft.EntityFrameworkCore;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Seeders;

public static class ProjectTagSeeder
{
    public static async Task<List<ProjectTag>> Seed(TimeForgeDbContext context, List<Project> projects, List<Tag> tags)
    {
        var projectTags = new List<ProjectTag>();
        var random = new Random();

        foreach (var project in projects)
        {
            var tagsToAssign = tags.OrderBy(x => random.Next()).Take(random.Next(1, 4)).ToList();

            foreach (var tag in tagsToAssign)
            {
                projectTags.Add(new ProjectTag { ProjectId = project.Id, TagId = tag.Id });
            }
        }

        await context.ProjectTags.AddRangeAsync(projectTags);
        await context.SaveChangesAsync();
        return projectTags;
    }
}