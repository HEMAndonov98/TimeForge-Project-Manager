using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.ViewModels.Task;

public class TaskViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    public bool IsCompleted { get; set; }

    public List<TimeEntryViewModel> TimeEntries { get; set; }
}