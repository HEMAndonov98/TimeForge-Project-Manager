using FastEndpoints;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Projects.Create;

public class CreateProjectEndpoint(TimeForgeDbContext db) : Endpoint<CreateProjectRequest, CreateProjectResponse>
{
    public override void Configure()
    {
        Post("/projects");
        Description(d => d
            .WithTags("Projects")
            .WithSummary("Create a new project")
            .Produces<CreateProjectResponse>(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401));
    }

    public override async Task HandleAsync(CreateProjectRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var project = Project.Create(
            userId,
            req.Name,
            req.Description,
            req.DueDate,
            req.Color);

        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);

        var response = new CreateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            DueDate = project.DueDate,
            Color = project.Color,
            Progress = project.Progress
        };

        await Send.CreatedAtAsync<CreateProjectEndpoint>(
            routeValues: new { id = project.Id },
            responseBody: response,
            generateAbsoluteUrl: true,
            cancellation: ct);
    }
}
