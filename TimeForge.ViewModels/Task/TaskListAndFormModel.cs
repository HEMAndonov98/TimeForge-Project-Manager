

namespace TimeForge.ViewModels.Task;

public class TaskListAndFormModel
{

    public string ProjectId { get; set; }

    public IEnumerable<TaskViewModel> Tasks { get; set; }
    public TaskInputModel TaskInputModel { get; set; }
}