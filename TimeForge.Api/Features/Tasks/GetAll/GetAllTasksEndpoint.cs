using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;
using TimeForge.Api.Features.Projects;

namespace TimeForge.Api.Features.Tasks.GetAll;

public class GetAllTasksRequest
{
    public string ProjectId { get; set; } = string.Empty;
}

public class GetAllTasksResponse
{
    public List<ProjectTaskDto> Tasks { get; set; } = new();
}

public class GetAllTasksEndpoint(TimeForgeDbContext db) : Endpoint<GetAllTasksRequest, GetAllTasksResponse>
{
    public override void Configure()
    {
        Get("projects/{ProjectId}/tasks");
        Description(d => d
            .WithTags("Tasks")
            .WithSummary("Get all tasks for a specific project")
            .Produces<GetAllTasksResponse>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(GetAllTasksRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.ProjectId && p.UserId == userId && !p.IsDeleted, ct);

        if (project == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var tasks = await db.Set<ProjectTask>()
        .AsNoTracking()
            .Where(t => t.ProjectId == req.ProjectId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var response = new GetAllTasksResponse
        {
            Tasks = tasks.Select(t => new ProjectTaskDto
            {
                Id = t.Id,
                Name = t.TaskName,
                Status = t.Status.ToString()
            }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
