using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class ProjectTask: BaseDeletableModel<string>
{
    public ProjectTask()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    
    [Required]
    [MaxLength(TaskValidationConstants.NameMaxLength)]
    public string Name { get; set; } = null!;
    
    //TODO Implement billing later
    public bool IsBillable { get; set; } = false;

    public bool IsCompleted { get; set; } = false;

    public DateTime CompletionDate { get; set; }

    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; } = null!;

    public Project Project { get; set; } = null!;
    
    [InverseProperty(nameof(TimeEntry.ProjectTask))]
    public virtual List<TimeEntry> TimeEntries { get; set; } = new();

}