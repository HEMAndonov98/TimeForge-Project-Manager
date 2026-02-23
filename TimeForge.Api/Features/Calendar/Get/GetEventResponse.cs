namespace TimeForge.Api.Features.Calendar.Get;

public class GetEventResponse
{
    public string Id { get; set; } = String.Empty;

    public string Title { get; set; } = String.Empty;

    public DateTime EventDate { get; set; }

    public string OwnerId { get; set; } = String.Empty;

    public string? ProjectId { get; set; } = String.Empty;
}