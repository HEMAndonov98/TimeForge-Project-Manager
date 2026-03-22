using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Projects.GetById;

public class GetProjectByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetProjectByIdResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int TasksDone { get; set; }
    public int TasksTotal { get; set; }

    public List<ProjectTaskDto> Tasks { get; set; } = new List<ProjectTaskDto>();
}

public class GetProjectByIdEndpoint(TimeForgeDbContext db) : Endpoint<GetProjectByIdRequest, GetProjectByIdResponse>
{
    public override void Configure()
    {
        Get("/projects/{id}");
        Description(d => d
            .WithTags("Projects")
            .WithSummary("Get a project by ID")
            .Produces<GetProjectByIdResponse>(200)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404));
    }

    public override async Task HandleAsync(GetProjectByIdRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var project = await db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == req.Id && p.UserId == userId, ct);

        if (project == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new GetProjectByIdResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            DueDate = project.DueDate,
            Color = project.Color,
            Progress = project.Progress,
            TasksDone = project.TasksDone,
            TasksTotal = project.TasksTotal,
            Tasks = project.Tasks.Select(t => new ProjectTaskDto
            {
                Id = t.Id,
                Name = t.TaskName,
                Status = t.Status.ToString()
            }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
