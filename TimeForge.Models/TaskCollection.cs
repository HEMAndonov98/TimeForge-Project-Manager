using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TaskCollection : BaseDeletableModel<string>
{
    [Required]
    [MaxLength(TaskListValidationConstants.ListTaskDescriptionMaxLength)]
    public string Description { get; set; } = null!;
    
    [ForeignKey(nameof(TaskItem))]
    public string TaskListId { get; set; }

    public TaskItem TaskItem { get; set; }

    public bool IsCompleted { get; set; }
}