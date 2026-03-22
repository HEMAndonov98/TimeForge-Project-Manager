using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Teams.GetAll;

public class GetTeamsResponse
{
    public List<TeamSummaryDto> Teams { get; set; } = new();
}

public class TeamSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string UserRole { get; set; } = string.Empty;
}

public class GetTeamsEndpoint(TimeForgeDbContext db) : EndpointWithoutRequest<GetTeamsResponse>
{
    public override void Configure()
    {
        Get("teams");
        Description(d => d
            .WithTags("Teams")
            .WithSummary("Get all teams for the current user")
            .Produces<GetTeamsResponse>(200)
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

        var teams = await db.Set<TeamMember>()
            .AsNoTracking()
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Include(m => m.Team)
                .ThenInclude(t => t.Members)
            .Select(m => new TeamSummaryDto
            {
                Id = m.Team.Id,
                Name = m.Team.Name,
                Description = m.Team.Description,
                MemberCount = m.Team.Members.Count(tm => !tm.IsDeleted),
                UserRole = m.Role.ToString()
            })
            .ToListAsync(ct);

        await Send.OkAsync(new GetTeamsResponse { Teams = teams }, ct);
    }
}
