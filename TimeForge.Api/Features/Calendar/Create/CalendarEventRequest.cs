namespace TimeForge.Api.Features.Calendar.Create;

public class CalendarEventRequest
{
    public string OwnerId { get; set; } = String.Empty;

    public string Title { get; set; } = String.Empty;

    public DateTime EventDate { get; set; }

    public string? ProjectId { get; set; }
}