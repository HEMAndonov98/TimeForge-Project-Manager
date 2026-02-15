using TimeForge.Models.Common;
using TaskStatus = TimeForge.Common.Enums.TaskStatus;

namespace TimeForge.Models;

/// <summary>
/// Represents a task within a project, including billing and completion status.
/// </summary>
public class ProjectTask : BaseDeletableModel<string>
{
    public ProjectTask() : base()
    {
    }

    public string TaskName { get; private set; } = string.Empty;

    public string ProjectId { get; private set; } = string.Empty;

    public Project Project { get; private set; } = null!;

    public TaskStatus Status { get; private set; }

    public ICollection<TimerSession> TimerSessions { get; private set; } = new List<TimerSession>();

    // Computed: Total time spent on this task
    public TimeSpan TotalTimeSpent => TimeSpan.FromSeconds(
        TimerSessions
            .Where(ts => ts.EndTime.HasValue && !ts.IsDeleted)
            .Sum(ts => ts.DurationInSeconds));

    public static ProjectTask Create(string projectId, string taskName)
    {
        return new ProjectTask()
        {
            TaskName = taskName,
            ProjectId = projectId,
            Status = TaskStatus.Todo
        };
    }

    public void UpdateStatus(TaskStatus newStatus)
    {
        Status = newStatus;
        this.MarkModified();
    }

    public void UpdateTaskName(string newName)
    {
        TaskName = newName;
        this.MarkModified();
    }
}