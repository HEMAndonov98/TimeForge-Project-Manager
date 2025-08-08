using System.ComponentModel.DataAnnotations.Schema;

namespace TimeForge.Models;

/// <summary>
/// Represents the association between a project and a tag.
/// </summary>
public class ProjectTag
{
    /// <summary>
/// Gets or sets the project ID.
/// </summary>
[ForeignKey(nameof(Project))]
public string ProjectId { get; set; } = null!;

    /// <summary>
/// Gets or sets the associated project.
/// </summary>
public Project Project { get; set; } = null!;

    /// <summary>
/// Gets or sets the tag ID.
/// </summary>
[ForeignKey(nameof(Tag))]
public string TagId { get; set; } = null!;

    /// <summary>
/// Gets or sets the associated tag.
/// </summary>
public Tag Tag { get; set; } = null!;
}