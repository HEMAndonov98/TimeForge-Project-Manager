using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TaskItem : BaseDeletableModel<string>
{
    [Required]
    [MaxLength(TaskCollectionConstants.TaskItemDescriptionMaxLength)]
    public string Title { get; set; } = null!;
    
    public bool IsCompleted { get; set; }
    
    
    [ForeignKey(nameof(TaskCollection))]
    public string TaskItemId { get; set; }

    public TaskCollection TaskCollection { get; set; }

    [ForeignKey(nameof(User))]
    public string UserId { get; set; }

    public User User { get; set; }
}