using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Projects.Delete;

public class DeleteProjectEndpoint(TimeForgeDbContext db) : Endpoint<DeleteProjectRequest>
{
    public override void Configure()
    {
        Delete("/projects/{id}");
        Description(d => d
            .WithTags("Projects")
            .WithSummary("Delete a project (soft delete)")
            .Produces(204)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(404));
    }

    public override async Task HandleAsync(DeleteProjectRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == req.Id && p.UserId == userId, ct);

        if (project == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        db.Projects.Remove(project); // Triggers SoftDeleteInterceptor
        await db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
