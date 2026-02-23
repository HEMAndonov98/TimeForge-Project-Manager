using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Database;

namespace TimeForge.Api.Features.Calendar.Get;

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
        Get("/calendar/event/{Id}");

        Description(d => d
        .WithName("CalendarEvent")
        .WithTags("Returns a single calendar event")
        .Produces<GetEventResponse>(200)
        .ProducesProblemDetails(StatusCodes.Status404NotFound)
        );
    }

    public override async Task HandleAsync(GetEventRequest req, CancellationToken ct)
    {
        var eventId = req.Id;

        if (string.IsNullOrEmpty(eventId))
        {
            _logger.LogWarning("Event ID is required");
            ThrowError("Event ID is required");
        }

        var calendarEvent = await _context.CalendarEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (calendarEvent == null)
        {
            _logger.LogWarning("Event not found: {EventId}", eventId);
            ThrowError("Event not found");
        }

        await Send.OkAsync(new GetEventResponse
        {
            Id = calendarEvent.Id,
            Title = calendarEvent.Title,
            EventDate = calendarEvent.EventDate,
            OwnerId = calendarEvent.OwnerId,
            ProjectId = calendarEvent.ProjectId
        },ct);
    }
}