using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TimeForge.ViewModels.Task;

public class TaskListAndFormModel
{
    [BindNever]
    [ValidateNever]
    public string ProjectId { get; set; }
    [BindNever]
    [ValidateNever]
    public IEnumerable<TaskViewModel> Tasks { get; set; }
    public TaskInputModel TaskInputModel { get; set; }
}