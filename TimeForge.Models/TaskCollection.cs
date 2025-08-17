using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TaskCollection : BaseDeletableModel<string>
{
    [Required]
    [MaxLength(TaskCollectionConstants.TaskCollectionNameMaxLength)]
    public string ListName { get; set; } = null!;

    [InverseProperty(nameof(TaskItem.TaskCollection))]
    public List<TaskItem> ListTasks { get; set; } = new();
}