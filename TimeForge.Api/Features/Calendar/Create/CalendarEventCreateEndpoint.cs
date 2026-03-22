using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Api.Features.Calendar.Get;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Calendar.Create;

public class CalendarEventRequest
{
    public string Title { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string? ProjectId { get; set; }
}

public class CreateEventValidator : Validator<CalendarEventRequest>
{
    public CreateEventValidator()
    {
        RuleFor(ce => ce.Title)
            .NotEmpty().WithMessage("The title is required.")
            .MaximumLength(100).WithMessage("Project name is too long");

    }
}

public class CalendarEventCreateEndpoint : Endpoint<CalendarEventRequest, CalendarEventResponse>
{
    private readonly TimeForgeDbContext _context;
    private readonly ILogger<CalendarEventCreateEndpoint> _logger;

    public CalendarEventCreateEndpoint(
        TimeForgeDbContext context,
        ILogger<CalendarEventCreateEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public override void Configure()
    {
        Post("calendar/events");
        Description(d => d
            .WithTags("CalendarEvent")
            .WithSummary("Creates a new event")
            .Produces<CalendarEventResponse>(201)
            .ProducesProblemDetails(StatusCodes.Status409Conflict)
            .ProducesProblemDetails(StatusCodes.Status400BadRequest)
        );
    }

    public override async Task HandleAsync(CalendarEventRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            ThrowError("Unauthorized", 401);
        }

        var eventExists = await _context.CalendarEvents
            .AsNoTracking()
            .AnyAsync(ce => ce.OwnerId == userId && ce.Title == req.Title && ce.EventDate == req.EventDate, ct);

        if (eventExists)
        {
            _logger.LogWarning("A calendar event with the same title already exists for this user: {Title}", req.Title);
            ThrowError("A calendar event with the same title and date already exists", 409);
        }
        
        var newEvent = CalendarEvent.Create(
            userId,
            req.Title,
            req.EventDate,
            req.ProjectId
        );

        try
        {
            await _context.CalendarEvents.AddAsync(newEvent, ct);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while creating a calendar event: {@Request}", req);
            ThrowError("An error occurred while saving the event", 500);
        }
        
        _logger.LogInformation("Created a calendar event: {EventId} for user {UserId}", newEvent.Id, userId);
        
        await Send.CreatedAtAsync<GetCalendarEventEndpoint>(
            routeValues: new { newEvent.Id },
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