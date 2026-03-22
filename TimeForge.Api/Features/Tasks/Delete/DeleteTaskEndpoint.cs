using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Tasks.Delete;

public class DeleteTaskRequest
{
    public string TaskId { get; set; } = string.Empty;
}

public class DeleteTaskEndpoint(TimeForgeDbContext db) : Endpoint<DeleteTaskRequest>
{
    public override void Configure()
    {
        Delete("tasks/{TaskId}");
        Description(d => d
            .WithTags("Tasks")
            .WithSummary("Delete a task")
            .Produces(204)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(DeleteTaskRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var task = await db.Set<ProjectTask>()
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == req.TaskId && !t.IsDeleted, ct);

        if (task == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (task.Project.UserId != userId)
        {
            await Send.NotFoundAsync(ct); // Avoid leaking task existence
            return;
        }

        task.MarkDeleted();
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
