using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;
using TimeForge.Common.Enums;

namespace TimeForge.Api.Features.Timer.Heartbeat;

public class HeartbeatEndpoint(TimeForgeDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("timer/heartbeat");
        Description(d => d
            .WithTags("Timer")
            .WithSummary("Send a heartbeat for the active timer")
            .Produces(204)
            .ProducesProblemDetails(404)
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

        var activeTimer = await db.Set<TimerSession>()
            .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.State == TimeEntryState.Running && !ts.IsDeleted, ct);

        if (activeTimer == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        activeTimer.Heartbeat();
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
