using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TaskCollection : BaseDeletableModel<string>
{
    public TaskCollection()
    {
        this.Id = Guid.NewGuid().ToString();
        this.CreatedAt = DateTime.UtcNow;
        this.LastModified = DateTime.UtcNow;
    }
    
    [Required]
    [MaxLength(TaskCollectionConstants.TaskCollectionNameMaxLength)]
    public string ListName { get; set; } = null!;

    
    [InverseProperty(nameof(TaskItem.TaskCollection))]
    public List<TaskItem> TaskItems { get; set; } = new();

    [ForeignKey(nameof(User))]
    public string UserId { get; set; }

    public User User { get; set; }
}