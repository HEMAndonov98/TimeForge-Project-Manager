using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TimeForge.Models.Common;

namespace TimeForge.Models;

/// <summary>
/// Represents a tag that can be assigned to projects.
/// </summary>
public class Tag : BaseDeletableModel<string>
{
    /// <summary>
/// Initializes a new instance of the <see cref="Tag"/> class with a new unique identifier.
/// </summary>
public Tag()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    
    /// <summary>
/// Gets or sets the name of the tag.
/// </summary>
[Required]
public string Name { get; set; } = null!;

    /// <summary>
/// Gets or sets the user ID of the tag's creator.
/// </summary>
[ForeignKey(nameof(CreatedBy))]
public string UserId { get; set; } = null!;

    /// <summary>
/// Gets or sets the user who created the tag.
/// </summary>
public User CreatedBy { get; set; } = null!;

    /// <summary>
/// Gets or sets the list of project-tag associations for this tag.
/// </summary>
[InverseProperty(nameof(ProjectTag.Tag))]
public virtual List<ProjectTag> ProjectTags { get; set; } = new();
}