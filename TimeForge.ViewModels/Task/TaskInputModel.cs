using System.ComponentModel.DataAnnotations;
using TimeForge.Common.Constants;
using TimeForge.Common.GlobalErrorMessages;

namespace TimeForge.ViewModels.Task;

public class TaskInputModel
{
    public string ProjectId { get; set; } = null!;
    [Required(ErrorMessage = TaskErrorMessages.TaskNameRequired)]
    [StringLength(TaskValidationConstants.NameMaxLength,
        MinimumLength = TaskValidationConstants.NameMinLength,
        ErrorMessage = TaskErrorMessages.TaskNameLength)]
    public string Name { get; set; } = null!;
}