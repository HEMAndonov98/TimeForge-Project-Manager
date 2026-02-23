using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Api.Features.Projects.Create;
using TimeForge.Database;

namespace TimeForge.Api.Features.Projects.Update;

public class UpdateProjectEndpoint(TimeForgeDbContext db) : Endpoint<UpdateProjectRequest, UpdateProjectResponse>
{
    public override void Configure()
    {
        Put("/projects/{id}");
        Description(d => d
            .WithTags("Projects")
            .WithSummary("Update an existing project")
            .Produces<UpdateProjectResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404));
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
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

        project.Update(
            req.Name,
            req.DueDate,
            project.Tasks.ToList(), // Keep existing tasks for now
            req.Description,
            req.Color);

        await db.SaveChangesAsync(ct);

        var response = new UpdateProjectResponse
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
