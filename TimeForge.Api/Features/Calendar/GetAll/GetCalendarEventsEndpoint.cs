using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Calendar.GetAll;

public class GetCalendarEventsRequest
{
    public string? ProjectId { get; set; }
}

public class CalendarEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? ProjectId { get; set; }
    public string ProjectColor { get; set; } = string.Empty;
}

public class GetCalendarEventsResponse
{
    public List<CalendarEventDto> Events { get; set; } = new();
}

public class GetCalendarEventsEndpoint : Endpoint<GetCalendarEventsRequest, GetCalendarEventsResponse>
{
    private readonly TimeForgeDbContext _context;

    public GetCalendarEventsEndpoint(TimeForgeDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("calendar/events");
        Description(d => d
            .WithTags("CalendarEvent")
            .WithSummary("Returns all calendar events for the user")
            .Produces<GetCalendarEventsResponse>(200)
        );
    }

    public override async Task HandleAsync(GetCalendarEventsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            ThrowError("Unauthorized", 401);
        }

        var query = _context.CalendarEvents
            .AsNoTracking()
            .Include(ce => ce.Project)
            .Where(ce => ce.OwnerId == userId);

        if (!string.IsNullOrEmpty(req.ProjectId))
        {
            query = query.Where(ce => ce.ProjectId == req.ProjectId);
        }

        var events = await query
            .OrderBy(ce => ce.EventDate)
            .Select(ce => new CalendarEventDto
            {
                Id = ce.Id,
                Title = ce.Title,
                EventDate = ce.EventDate,
                ProjectId = ce.ProjectId,
                ProjectColor = ce.ProjectColor
            })
            .ToListAsync(ct);

        await Send.OkAsync(new GetCalendarEventsResponse { Events = events }, ct);
    }
}
