using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Teams.RemoveMember;

public class RemoveTeamMemberRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class RemoveTeamMemberEndpoint(TimeForgeDbContext db) : Endpoint<RemoveTeamMemberRequest>
{
    public override void Configure()
    {
        Delete("teams/{TeamId}/members/{UserId}");
        Description(d => d
            .WithTags("Teams")
            .WithSummary("Remove a member from the team or leave the team")
            .Produces(204)
            .ProducesProblemDetails(403)
            .ProducesProblemDetails(404));
    }

    public override async Task HandleAsync(RemoveTeamMemberRequest req, CancellationToken ct)
    {
        var callerUserId = User.GetUserId();
        if (string.IsNullOrEmpty(callerUserId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var team = await db.Set<Team>()
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == req.TeamId && !t.IsDeleted, ct);

        if (team == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var callerMembership = team.Members.FirstOrDefault(m => m.UserId == callerUserId && !m.IsDeleted);
        var targetMembership = team.Members.FirstOrDefault(m => m.UserId == req.UserId && !m.IsDeleted);

        if (callerMembership == null || targetMembership == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Logic check:
        // 1. Can remove self (unless Owner)
        // 2. Owner can remove anyone except themselves
        // 3. Manager can remove anyone except Owner

        bool canRemove = false;

        if (callerUserId == req.UserId) 
        {
            // Self removal
            if (callerMembership.Role == TeamRole.Owner)
            {
                ThrowError("Owner cannot leave the team. Transfer ownership first.");
            }
            canRemove = true;
        }
        else if (callerMembership.Role == TeamRole.Owner)
        {
            // Owner can remove anyone else
            canRemove = true;
        }
        else if (callerMembership.Role == TeamRole.Manager)
        {
            // Manager can remove non-owners
            if (targetMembership.Role != TeamRole.Owner)
            {
                canRemove = true;
            }
        }

        if (!canRemove)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        team.RemoveMember(req.UserId);
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
