using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Database;

namespace TimeForge.Api.Features.Calendar.Delete;

public class CalendarEventDeleteEndpoint : Endpoint<CalendarEventDeleteRequest>
{
    private readonly TimeForgeDbContext context;
    private readonly ILogger<CalendarEventDeleteEndpoint> logger;

    public CalendarEventDeleteEndpoint(
        TimeForgeDbContext context,
        ILogger<CalendarEventDeleteEndpoint> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public override void Configure()
    {
        Delete("calendar/delete/event/{Id}");
        
        Description(d => d
            .WithTags("CalendarEvent")
            .WithSummary("Deletes an event")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblemDetails(StatusCodes.Status404NotFound)
        );
    }

    public override async Task HandleAsync(CalendarEventDeleteRequest req, CancellationToken ct)
    {
        var calendarEvent = await this.context.CalendarEvents
            .FirstOrDefaultAsync(ce => ce.Id == req.Id, ct);

        if (calendarEvent == null)
        {
            this.logger.LogWarning("Calendar event not found for deletion: {EventId}", req.Id);
            await Send.NotFoundAsync(ct);
            return;
        }

        try
        {
            this.context.CalendarEvents.Remove(calendarEvent);
            await this.context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "An error occurred while deleting calendar event: {EventId}", req.Id);
            ThrowError("An error occurred while deleting the calendar event");
        }

        this.logger.LogInformation("Deleted calendar event: {EventId}", req.Id);
        await Send.NoContentAsync(ct);
    }
}
