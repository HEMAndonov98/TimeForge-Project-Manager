using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Calendar.Delete;

public class CalendarEventDeleteRequest
{
    public string Id { get; set; } = string.Empty;
}

public class CalendarEventDeleteEndpoint : Endpoint<CalendarEventDeleteRequest>
{
    private readonly TimeForgeDbContext _context;
    private readonly ILogger<CalendarEventDeleteEndpoint> _logger;

    public CalendarEventDeleteEndpoint(
        TimeForgeDbContext context,
        ILogger<CalendarEventDeleteEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("calendar/events/{Id}");
        Description(d => d
            .WithTags("CalendarEvent")
            .WithSummary("Deletes an event")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblemDetails(StatusCodes.Status404NotFound)
            .ProducesProblemDetails(StatusCodes.Status403Forbidden)
        );
    }

    public override async Task HandleAsync(CalendarEventDeleteRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            ThrowError("Unauthorized", 401);
        }

        var calendarEvent = await _context.CalendarEvents
            .FirstOrDefaultAsync(ce => ce.Id == req.Id, ct);

        if (calendarEvent == null)
        {
            _logger.LogWarning("Calendar event not found for deletion: {EventId}", req.Id);
            await Send.NotFoundAsync(ct);
            return;
        }

        if (calendarEvent.OwnerId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete event {EventId} owned by {OwnerId}", userId, req.Id, calendarEvent.OwnerId);
            ThrowError("Forbidden", 403);
        }

        try
        {
            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while deleting calendar event: {EventId}", req.Id);
            ThrowError("An error occurred while deleting the calendar event", 500);
        }

        _logger.LogInformation("Deleted calendar event: {EventId} for user {UserId}", req.Id, userId);
        await Send.NoContentAsync(ct);
    }
}
