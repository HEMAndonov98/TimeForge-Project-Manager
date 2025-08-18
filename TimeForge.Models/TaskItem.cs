using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TaskItem : BaseDeletableModel<string>
{
    public TaskItem()
    {
        this.Id = Guid.NewGuid().ToString();
        this.CreatedAt = DateTime.UtcNow;
        this.LastModified = DateTime.UtcNow;
    }
    
    [Required]
    [MaxLength(TaskCollectionConstants.TaskItemTitleMaxLength)]
    public string Title { get; set; } = null!;
    
    public bool IsCompleted { get; set; }
    
    
    [ForeignKey(nameof(TaskCollection))]
    public string TaskCollectionId { get; set; }

    public TaskCollection TaskCollection { get; set; }
}