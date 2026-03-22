using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;
using TimeForge.Common.Enums;

namespace TimeForge.Api.Features.Timer.Stop;

public class StopTimerRequest
{
    public bool Finish { get; set; } = false; // true = Completed, false = Paused
}

public class StopTimerResponse
{
    public string Id { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int TotalSeconds { get; set; }
}

public class StopTimerEndpoint(TimeForgeDbContext db) : Endpoint<StopTimerRequest, StopTimerResponse>
{
    public override void Configure()
    {
        Post("timer/stop");
        Description(d => d
            .WithTags("Timer")
            .WithSummary("Pause or finish the active timer")
            .Produces<StopTimerResponse>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(StopTimerRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var activeTimer = await db.Set<TimerSession>()
            .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.State == TimeEntryState.Running && !ts.IsDeleted, ct);

        if (activeTimer == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (req.Finish)
        {
            activeTimer.Stop();
        }
        else
        {
            activeTimer.Pause();
        }

        await db.SaveChangesAsync(ct);

        var response = new StopTimerResponse
        {
            Id = activeTimer.Id,
            State = activeTimer.State.ToString(),
            TotalSeconds = activeTimer.TotalSeconds
        };

        await Send.OkAsync(response, ct);
    }
}
