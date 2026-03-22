using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;
using TimeForge.Common.Enums;

namespace TimeForge.Api.Features.Timer.GetStatus;

public class TimerStatusResponse
{
    public string? Id { get; set; }
    public string? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string State { get; set; } = "None";
    public int TotalSeconds { get; set; }
    public DateTime? LastStartedAt { get; set; }
    public int CurrentSessionSeconds => State == "Running" && LastStartedAt.HasValue 
        ? (int)(DateTime.UtcNow - LastStartedAt.Value).TotalSeconds 
        : 0;
}

public class GetStatusEndpoint(TimeForgeDbContext db) : EndpointWithoutRequest<TimerStatusResponse>
{
    public override void Configure()
    {
        Get("timer/status");
        Description(d => d
            .WithTags("Timer")
            .WithSummary("Get current timer status and handle recovery")
            .Produces<TimerStatusResponse>(200)
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

        // Find active or paused timer
        var timer = await db.Set<TimerSession>()
            .Include(ts => ts.Task)
            .FirstOrDefaultAsync(ts => ts.UserId == userId && (ts.State == TimeEntryState.Running || ts.State == TimeEntryState.Paused) && !ts.IsDeleted, ct);

        if (timer == null)
        {
            await Send.OkAsync(new TimerStatusResponse { State = "None" }, ct);
            return;
        }

        // Recovery Logic: If running but heartbeat is stale (> 60s)
        if (timer.State == TimeEntryState.Running)
        {
            var stalenessLimit = DateTime.UtcNow.AddSeconds(-60);
            if (timer.LastHeartbeat < stalenessLimit)
            {
                // Auto-pause due to inactivity
                timer.Pause();
                await db.SaveChangesAsync(ct);
            }
        }

        var response = new TimerStatusResponse
        {
            Id = timer.Id,
            TaskId = timer.TaskId,
            TaskTitle = timer.Task?.TaskName,
            State = timer.State.ToString(),
            TotalSeconds = timer.TotalSeconds,
            LastStartedAt = timer.LastStartedAt
        };

        await Send.OkAsync(response, ct);
    }
}
