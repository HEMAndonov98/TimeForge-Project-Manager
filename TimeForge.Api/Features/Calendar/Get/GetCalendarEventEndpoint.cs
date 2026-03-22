using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Calendar.Get;

public class GetEventRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetEventResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string? ProjectId { get; set; }
}

public class GetCalendarEventEndpoint : Endpoint<GetEventRequest, GetEventResponse>
{
    private readonly TimeForgeDbContext _context;
    private readonly ILogger<GetCalendarEventEndpoint> _logger;

    public GetCalendarEventEndpoint(TimeForgeDbContext context, ILogger<GetCalendarEventEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("calendar/events/{Id}");
        Description(d => d
            .WithTags("CalendarEvent")
            .WithSummary("Returns a single calendar event")
            .Produces<GetEventResponse>(200)
            .ProducesProblemDetails(StatusCodes.Status404NotFound)
            .ProducesProblemDetails(StatusCodes.Status403Forbidden)
        );
    }

    public override async Task HandleAsync(GetEventRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            ThrowError("Unauthorized", 401);
        }

        var calendarEvent = await _context.CalendarEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == req.Id, ct);

        if (calendarEvent == null)
        {
            _logger.LogWarning("Event not found: {EventId}", req.Id);
            await Send.NotFoundAsync(ct);
            return;
        }

        if (calendarEvent.OwnerId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access event {EventId} owned by {OwnerId}", userId, req.Id, calendarEvent.OwnerId);
            ThrowError("Forbidden", 403);
        }

        await Send.OkAsync(new GetEventResponse
        {
            Id = calendarEvent.Id,
            Title = calendarEvent.Title,
            EventDate = calendarEvent.EventDate,
            OwnerId = calendarEvent.OwnerId,
            ProjectId = calendarEvent.ProjectId
        }, ct);
    }
}