using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Tasks.UpdateStatus;

public class UpdateTaskStatusEndpoint(TimeForgeDbContext db) 
    : Endpoint<UpdateTaskStatusRequest, UpdateTaskStatusResponse>
{
    public override void Configure()
    {
        Patch("tasks/{TaskId}/status");
        AllowAnonymous(); // Temporarily for dev or if the project doesn't have strict auth yet? 
        // Actually, the context says "Authentication: JWT Bearer Tokens". 
        // Let's use it.
    }

    public override async Task HandleAsync(UpdateTaskStatusRequest req, CancellationToken ct)
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
            await Send.NotFoundAsync(ct); // Return NotFound instead of Forbidden to avoid leaking task existence
            return;
        }

        task.UpdateStatus(req.NewStatus);
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(new UpdateTaskStatusResponse
        {
            Id = task.Id,
            TaskName = task.TaskName,
            Status = task.Status
        }, ct);
    }
}
