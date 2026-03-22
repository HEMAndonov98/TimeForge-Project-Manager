using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Calendar.Update;

public class CalendarEventUpdateRequest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? ProjectId { get; set; }
}

public class CalendarEventUpdateResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? ProjectId { get; set; }
}

public class UpdateEventValidator : Validator<CalendarEventUpdateRequest>
{
    public UpdateEventValidator()
    {
        RuleFor(ce => ce.Title)
            .NotEmpty().WithMessage("The title is required.")
            .MaximumLength(100).WithMessage("Title is too long");
    }
}

public class CalendarEventUpdateEndpoint : Endpoint<CalendarEventUpdateRequest, CalendarEventUpdateResponse>
{
    private readonly TimeForgeDbContext _context;
    private readonly ILogger<CalendarEventUpdateEndpoint> _logger;

    public CalendarEventUpdateEndpoint(TimeForgeDbContext context, ILogger<CalendarEventUpdateEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("calendar/events/{Id}");
        Description(d => d
            .WithTags("CalendarEvent")
            .WithSummary("Updates an existing calendar event")
            .Produces<CalendarEventUpdateResponse>(200)
            .ProducesProblemDetails(StatusCodes.Status404NotFound)
            .ProducesProblemDetails(StatusCodes.Status403Forbidden)
            .ProducesProblemDetails(StatusCodes.Status400BadRequest)
        );
    }

    public override async Task HandleAsync(CalendarEventUpdateRequest req, CancellationToken ct)
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
            _logger.LogWarning("Calendar event not found for update: {EventId}", req.Id);
            await Send.NotFoundAsync(ct);
            return;
        }

        if (calendarEvent.OwnerId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update event {EventId} owned by {OwnerId}", userId, req.Id, calendarEvent.OwnerId);
            ThrowError("Forbidden", 403);
        }

        try
        {
            calendarEvent.Update(req.Title, req.EventDate, req.ProjectId);
            await _context.SaveChangesAsync(ct);
        }
        catch (ArgumentException ex)
        {
            ThrowError(ex.Message, 400);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while updating calendar event: {EventId}", req.Id);
            ThrowError("An error occurred while updating the calendar event", 500);
        }

        await Send.OkAsync(new CalendarEventUpdateResponse
        {
            Id = calendarEvent.Id,
            Title = calendarEvent.Title,
            EventDate = calendarEvent.EventDate,
            ProjectId = calendarEvent.ProjectId
        }, ct);
    }
}
