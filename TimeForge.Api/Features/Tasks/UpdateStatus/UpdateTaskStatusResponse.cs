using TaskStatus = TimeForge.Common.Enums.TaskStatus;

namespace TimeForge.Api.Features.Tasks.UpdateStatus;

public class UpdateTaskStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}
