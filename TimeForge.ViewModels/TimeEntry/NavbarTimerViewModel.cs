using TimeForge.Common.Enums;

namespace TimeForge.ViewModels.TimeEntry;

public class NavbarTimerViewModel
{
    public string Id { get; set; }

    public string TaskName { get; set; }

    public long DurationInMiliseconds { get; set; }

    public TimeEntryState State { get; set; }

    public bool IsRunning => State == TimeEntryState.Running;
}