using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;
using TimeForge.Common.Enums;

namespace TimeForge.Api.Features.Timer.Start;

public class StartTimerRequest
{
    public string TaskId { get; set; } = string.Empty;
}

public class StartTimerResponse
{
    public string Id { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int TotalSeconds { get; set; }
}

public class StartTimerEndpoint(TimeForgeDbContext db) : Endpoint<StartTimerRequest, StartTimerResponse>
{
    public override void Configure()
    {
        Post("timer/start");
        Description(d => d
            .WithTags("Timer")
            .WithSummary("Start or resume a timer for a task")
            .Produces<StartTimerResponse>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(StartTimerRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        // 1. Find and pause any active timer
        var activeTimer = await db.Set<TimerSession>()
            .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.State == TimeEntryState.Running && !ts.IsDeleted, ct);

        if (activeTimer != null)
        {
            activeTimer.Pause();
        }

        // 2. Check if a timer for this task already exists for this user (Paused)
        var existingTimer = await db.Set<TimerSession>()
            .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.TaskId == req.TaskId && ts.State == TimeEntryState.Paused && !ts.IsDeleted, ct);

        TimerSession currentTimer;
        if (existingTimer != null)
        {
            existingTimer.Resume();
            currentTimer = existingTimer;
        }
        else
        {
            // Verify task exists
            var taskExists = await db.Set<ProjectTask>().AsNoTracking().AnyAsync(t => t.Id == req.TaskId && !t.IsDeleted, ct);
            if (!taskExists)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            currentTimer = TimerSession.Start(userId, req.TaskId);
            await db.AddAsync(currentTimer, ct);
        }

        await db.SaveChangesAsync(ct);

        var response = new StartTimerResponse
        {
            Id = currentTimer.Id,
            TaskId = currentTimer.TaskId,
            State = currentTimer.State.ToString(),
            TotalSeconds = currentTimer.TotalSeconds
        };

        await Send.OkAsync(response, ct);
    }
}
