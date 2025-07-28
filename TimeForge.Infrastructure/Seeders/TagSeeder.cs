using Microsoft.EntityFrameworkCore;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Seeders;

public static class TagSeeder
{
    public static async Task<List<Tag>> Seed(TimeForgeDbContext context, string userId)
    {
        var tags = new List<Tag>();
        
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Web Development", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Mobile App", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Database", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "UI/UX Design", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Marketing", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Content Creation", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "QA Testing", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Client Meeting", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Backend Development", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "Frontend Development", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "API Integration", UserId = userId, CreatedAt = DateTime.UtcNow });
        tags.Add(new Tag { Id = Guid.NewGuid().ToString(), Name = "DevOps", UserId = userId, CreatedAt = DateTime.UtcNow });

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
        return tags;
    }
}