using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Projects.Update;

public class UpdateProjectRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class UpdateProjectResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int TasksDone { get; set; }
    public int TasksTotal { get; set; }
}

public class UpdateProjectValidator : Validator<UpdateProjectRequest>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(100).WithMessage("Project name is too long");
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description is too long");
    }
}

public class UpdateProjectEndpoint(TimeForgeDbContext db) : Endpoint<UpdateProjectRequest, UpdateProjectResponse>
{
    public override void Configure()
    {
        Put("/projects/{id}");
        Description(d => d
            .WithTags("Projects")
            .WithSummary("Update an existing project")
            .Produces<UpdateProjectResponse>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404));
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var project = await db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == req.Id && p.UserId == userId, ct);

        if (project == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        project.Update(
            req.Name,
            req.DueDate,
            project.Tasks.ToList(), // Keep existing tasks for now
            req.Description,
            req.Color);

        await db.SaveChangesAsync(ct);

        var response = new UpdateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            DueDate = project.DueDate,
            Color = project.Color,
            Progress = project.Progress,
            TasksDone = project.TasksDone,
            TasksTotal = project.TasksTotal
        };

        await Send.OkAsync(response, ct);
    }
}
