using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

/// <summary>
/// Represents a project entity with tasks, tags, and ownership information.
/// </summary>
public class Project : BaseDeletableModel<string>
{
    /// <summary>
/// Initializes a new instance of the <see cref="Project"/> class with a new unique identifier.
/// </summary>
public Project()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    
    [Required]
    [MaxLength(ProjectValidationConstants.NameMaxLength)]
    /// <summary>
/// Gets or sets the name of the project.
/// </summary>
public string Name { get; set; } = null!;

    /// <summary>
/// Gets or sets a value indicating whether the project is public.
/// </summary>
public bool IsPublic { get; set; } = true;

    /// <summary>
/// Gets or sets the due date of the project.
/// </summary>
public DateOnly? DueDate { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    /// <summary>
/// Gets or sets the user ID of the project's creator.
/// </summary>
public string UserId { get; set; } = null!;

    /// <summary>
/// Gets or sets the user who created the project.
/// </summary>
public User CreatedBy { get; set; } = null!;

    [InverseProperty(nameof(ProjectTask.Project))]
    /// <summary>
/// Gets or sets the list of tasks associated with the project.
/// </summary>
public virtual List<ProjectTask> Tasks { get; set; } = new();

    [InverseProperty(nameof(ProjectTag.Project))]
    /// <summary>
/// Gets or sets the list of tags associated with the project.
/// </summary>
public virtual List<ProjectTag> ProjectTags { get; set; } = new();
}