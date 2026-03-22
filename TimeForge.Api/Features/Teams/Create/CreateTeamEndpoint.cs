using FastEndpoints;
using FluentValidation;
using TimeForge.Api.Common.Extensions;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Teams.Create;

public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateTeamResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateTeamValidator : Validator<CreateTeamRequest>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(100).WithMessage("Team name is too long.");
    }
}

public class CreateTeamEndpoint(TimeForgeDbContext db) : Endpoint<CreateTeamRequest, CreateTeamResponse>
{
    public override void Configure()
    {
        Post("teams");
        Description(d => d
            .WithTags("Teams")
            .WithSummary("Create a new team")
            .Produces<CreateTeamResponse>(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(CreateTeamRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var team = Team.Create(req.Name, req.Description);
        
        // Add creator as Owner
        team.AddMember(userId, TeamRole.Owner);

        db.Set<Team>().Add(team);
        await db.SaveChangesAsync(ct);

        await Send.CreatedAtAsync<CreateTeamEndpoint>(null, new CreateTeamResponse
        {
            Id = team.Id,
            Name = team.Name
        }, cancellation: ct);
    }
}
