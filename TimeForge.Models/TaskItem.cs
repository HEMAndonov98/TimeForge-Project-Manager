using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TaskItem : BaseDeletableModel<string>
{
    [Required]
    [MaxLength(TaskListValidationConstants.TaskListNameMaxLength)]
    public string ListName { get; set; } = null!;

    [InverseProperty(nameof(TaskCollection.TaskItem))]
    public List<TaskCollection> ListTasks { get; set; } = new();
}