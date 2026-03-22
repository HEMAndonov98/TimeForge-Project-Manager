using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;
using TaskStatus = TimeForge.Common.Enums.TaskStatus;

namespace TimeForge.Api.Features.Tasks.UpdateStatus;

public class UpdateTaskStatusRequest
{
    public string TaskId { get; set; } = string.Empty;
    public TaskStatus NewStatus { get; set; }
}

public class UpdateTaskStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}

public class UpdateTaskStatusValidator : Validator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("A valid status is required.");
    }
}

public class UpdateTaskStatusEndpoint(TimeForgeDbContext db) 
    : Endpoint<UpdateTaskStatusRequest, UpdateTaskStatusResponse>
{
    public override void Configure()
    {
        Patch("tasks/{TaskId}/status");
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
