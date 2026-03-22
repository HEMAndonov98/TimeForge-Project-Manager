using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Teams.UpdateRole;

public class UpdateMemberRoleRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public TeamRole NewRole { get; set; }
}

public class UpdateMemberRoleValidator : Validator<UpdateMemberRoleRequest>
{
    public UpdateMemberRoleValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewRole).IsInEnum();
    }
}

public class UpdateMemberRoleEndpoint(TimeForgeDbContext db) : Endpoint<UpdateMemberRoleRequest>
{
    public override void Configure()
    {
        Patch("teams/{TeamId}/members/{UserId}/role");
        Description(d => d
            .WithTags("Teams")
            .WithSummary("Update a team member's role or transfer ownership")
            .Produces(204)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(403)
            .ProducesProblemDetails(404));
    }

    public override async Task HandleAsync(UpdateMemberRoleRequest req, CancellationToken ct)
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
        if (callerMembership == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Only Owner can manage roles
        if (callerMembership.Role != TeamRole.Owner)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        try
        {
            if (req.NewRole == TeamRole.Owner)
            {
                // Ownership transfer (swap)
                team.TransferOwnership(callerUserId, req.UserId);
            }
            else
            {
                // Simple role update
                team.UpdateMemberRole(req.UserId, req.NewRole);
            }

            await db.SaveChangesAsync(ct);
            await Send.NoContentAsync(ct);
        }
        catch (ArgumentException ex)
        {
            ThrowError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ThrowError(ex.Message);
        }
    }
}
