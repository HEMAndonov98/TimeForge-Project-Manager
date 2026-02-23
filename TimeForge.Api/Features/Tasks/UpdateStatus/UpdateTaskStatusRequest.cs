using TaskStatus = TimeForge.Common.Enums.TaskStatus;

namespace TimeForge.Api.Features.Tasks.UpdateStatus;

public class UpdateTaskStatusRequest
{
    public string TaskId { get; set; } = string.Empty;
    public TaskStatus NewStatus { get; set; }
}
