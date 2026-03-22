using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Teams.AddMember;

public class AddTeamMemberRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}

public class AddTeamMemberValidator : Validator<AddTeamMemberRequest>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
    }
}

public class AddTeamMemberEndpoint(TimeForgeDbContext db) : Endpoint<AddTeamMemberRequest>
{
    public override void Configure()
    {
        Post("teams/{TeamId}/members");
        Description(d => d
            .WithTags("Teams")
            .WithSummary("Add a member to the team")
            .Produces(204)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(AddTeamMemberRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // 1. Verify caller has permission (Owner or Manager)
        var callerMembership = await db.Set<TeamMember>()
            .FirstOrDefaultAsync(m => m.TeamId == req.TeamId && m.UserId == userId && !m.IsDeleted, ct);

        if (callerMembership == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (callerMembership.Role != TeamRole.Owner && callerMembership.Role != TeamRole.Manager)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        // 2. Find user to add
        var userToAdd = await db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == req.UserEmail.ToLowerInvariant() && !u.IsDeleted, ct);

        if (userToAdd == null)
        {
            ThrowError(r => r.UserEmail, "User with this email not found.");
        }

        // 3. Check if already a member
        var existingMember = await db.Set<TeamMember>()
            .AnyAsync(m => m.TeamId == req.TeamId && m.UserId == userToAdd.Id && !m.IsDeleted, ct);

        if (existingMember)
        {
            ThrowError(r => r.UserEmail, "User is already a member of this team.");
        }

        // 4. Add member
        var team = await db.Set<Team>()
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == req.TeamId && !t.IsDeleted, ct);

        if (team == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        team.AddMember(userToAdd.Id);
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
