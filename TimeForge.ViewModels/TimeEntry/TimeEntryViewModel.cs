using TimeForge.Common.Enums;

namespace TimeForge.ViewModels.TimeEntry;

public class TimeEntryViewModel
{
    public string Id { get; set; }
    public DateTime Start { get; set; }

    public DateTime? End { get; set; }

    public string TaskName { get; set; }

    public string TaskId { get; set; }

    public string CreatedBy { get; set; }

    public TimeSpan Duration { get; set; }

    public TimeEntryState State { get; set; }

}