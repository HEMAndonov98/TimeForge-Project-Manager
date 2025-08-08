using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

/// <summary>
/// Represents a task within a project, including billing and completion status.
/// </summary>
public class ProjectTask: BaseDeletableModel<string>
{
    /// <summary>
/// Initializes a new instance of the <see cref="ProjectTask"/> class with a new unique identifier.
/// </summary>
public ProjectTask()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    

    /// <summary>
/// Gets or sets the name of the task.
/// </summary>
[Required]
[MaxLength(TaskValidationConstants.NameMaxLength)]
public string Name { get; set; } = null!;
    
    //TODO Implement billing later
    /// <summary>
/// Gets or sets a value indicating whether the task is billable.
/// </summary>
public bool IsBillable { get; set; }

    /// <summary>
/// Gets or sets a value indicating whether the task is completed.
/// </summary>
public bool IsCompleted { get; set; }

    //TODO implement new migration
    /// <summary>
/// Gets or sets the completion date of the task.
/// </summary>
public DateTime? CompletionDate { get; set; }

    /// <summary>
/// Gets or sets the project ID this task belongs to.
/// </summary>
[ForeignKey(nameof(Project))]
public string ProjectId { get; set; } = null!;

    /// <summary>
/// Gets or sets the project this task belongs to.
/// </summary>
public Project Project { get; set; } = null!;
    
    /// <summary>
/// Gets or sets the list of time entries associated with this task.
/// </summary>
[InverseProperty(nameof(TimeEntry.ProjectTask))]
public virtual List<TimeEntry> TimeEntries { get; set; } = new();

}