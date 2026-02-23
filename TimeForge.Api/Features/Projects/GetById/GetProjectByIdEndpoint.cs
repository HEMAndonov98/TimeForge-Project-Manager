using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Api.Features.Projects.Create; // Using CreateProjectResponse for consistency or define a specific one
using TimeForge.Database;

namespace TimeForge.Api.Features.Projects.GetById;

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
            TasksTotal = project.TasksTotal
        };

        await Send.OkAsync(response, ct);
    }
}
