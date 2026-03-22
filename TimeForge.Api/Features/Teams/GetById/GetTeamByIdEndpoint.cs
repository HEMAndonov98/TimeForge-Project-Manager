using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Teams.GetById;

public class GetTeamByIdRequest
{
    public string TeamId { get; set; } = string.Empty;
}

public class GetTeamByIdResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TeamMemberDetailDto> Members { get; set; } = new();
}

public class TeamMemberDetailDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class GetTeamByIdEndpoint(TimeForgeDbContext db) : Endpoint<GetTeamByIdRequest, GetTeamByIdResponse>
{
    public override void Configure()
    {
        Get("teams/{TeamId}");
        Description(d => d
            .WithTags("Teams")
            .WithSummary("Get team details by ID")
            .Produces<GetTeamByIdResponse>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(GetTeamByIdRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Verify membership first
        var isMember = await db.Set<TeamMember>()
            .AnyAsync(m => m.TeamId == req.TeamId && m.UserId == userId && !m.IsDeleted, ct);

        if (!isMember)
        {
            await Send.NotFoundAsync(ct); // Hide existence
            return;
        }

        var team = await db.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
            .FirstOrDefaultAsync(t => t.Id == req.TeamId && !t.IsDeleted, ct);

        if (team == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new GetTeamByIdResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Members = team.Members
                .Where(m => !m.IsDeleted)
                .Select(m => new TeamMemberDetailDto
                {
                    UserId = m.UserId,
                    FullName = m.User.FullName,
                    Role = m.Role.ToString()
                }).ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
