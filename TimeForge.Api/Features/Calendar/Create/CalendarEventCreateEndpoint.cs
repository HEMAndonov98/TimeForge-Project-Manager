using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Features.Calendar.Get;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Calendar.Create;

public class CalendarEventCreateEndpoint : Endpoint<CalendarEventRequest, CalendarEventResponse>
{
    private readonly TimeForgeDbContext context;
    private readonly ILogger<CalendarEventCreateEndpoint> logger;

    public CalendarEventCreateEndpoint(
        TimeForgeDbContext context,
        ILogger<CalendarEventCreateEndpoint> logger)
    {
        this.context = context;
        this.logger = logger;
    }
    
    public override void Configure()
    {
       Post("calendar/create/event");

       Description(d => d
           .WithTags("CalendarEvent")
           .WithSummary("Creates a new event")
           .Produces<CalendarEventResponse>(201)
           .ProducesProblemDetails(StatusCodes.Status409Conflict)
       );
    }

    public override async Task HandleAsync(CalendarEventRequest req, CancellationToken ct)
    {
        var eventExists = context.CalendarEvents
            .AsNoTracking()
            .FirstOrDefault(ce => ce.Title == req.Title && ce.EventDate == req.EventDate);

        if (eventExists != null)
        {
            logger.LogWarning("A calendar event with the same title already exists: {@Event}", req);
            HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            ThrowError("A calendar event with the same title already exists");
        }
        
        var newEvent = CalendarEvent.Create(
            req.OwnerId,
            req.Title,
            req.EventDate,
            req.ProjectId
            );

        try
        {
            await context.CalendarEvents.AddAsync(newEvent, ct);
            await context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
           logger.LogWarning("An error occurred while creating a calendar event: {@Event}", req);
            ThrowError(e.Message);
        }
        
        logger.LogInformation("Created a calendar event: {@Event}", req);
        await Send.CreatedAtAsync<GetCalendarEventEndpoint>(
            routeValues: null,
            responseBody: new CalendarEventResponse()
            {
                Id = newEvent.Id,
                Title = newEvent.Title,
                EventDate = newEvent.EventDate,
                ProjectId = newEvent.ProjectId,
                OwnerId = newEvent.OwnerId
            },
            generateAbsoluteUrl: true,
            cancellation: ct);
    }
}