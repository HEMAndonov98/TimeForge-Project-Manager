using TimeForge.Models.Common;

namespace TimeForge.Models;

public class CalendarEvent : BaseDeletableModel<string>
{
    public CalendarEvent() : base()
    {
    }
    public int OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;
    public string? ProjectId { get; private set; }
    public Project? Project { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime EventDate { get; private set; }

    // NEW: Computed property for UI
    public string ProjectColor => Project?.Color ?? "gray";

    public static CalendarEvent Create(int ownerId, string title, DateTime eventDate, string? projectId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Calendar event title is required");
        }

        return new CalendarEvent
        {
            OwnerId = ownerId,
            Title = title,
            EventDate = eventDate,
            ProjectId = projectId
        };
    }

    public void Update(
        string title,
        DateTime eventDate,
        string? projectId = null
        )
    {

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Calendar event title is required");
        }

        Title = title;
        EventDate = eventDate;
        ProjectId = projectId;
        this.MarkModified();
    }
}