using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Projects.GetAll;

public class GetProjectsEndpoint(TimeForgeDbContext db) : EndpointWithoutRequest<GetProjectsResponse>
{
    public override void Configure()
    {
        Get("/projects");
        Description(d => d
            .WithTags("Projects")
            .WithSummary("Get all projects for the current user")
            .Produces<GetProjectsResponse>(200)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var projects = await db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var response = new GetProjectsResponse
        {
            Projects = projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DueDate = p.DueDate,
                Color = p.Color,
                Progress = p.Progress,
                TasksDone = p.TasksDone,
                TasksTotal = p.TasksTotal
            }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
