using TimeForge.ViewModels.Task;

namespace TimeForge.Services.Interfaces;

public interface ITaskService
{
    Task CreateTaskAsync(TaskInputModel inputModel);
    Task CompleteTask(string taskId);
    Task<TaskViewModel> GetTaskByIdAsync(string taskId);
    Task<IEnumerable<TaskViewModel>> GetTasksByProjectIdAsync(string projectId);
    Task DeleteTaskAsync(string taskId);
}