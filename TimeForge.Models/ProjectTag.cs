using System.ComponentModel.DataAnnotations.Schema;

namespace TimeForge.Models;

public class ProjectTag
{
    [ForeignKey(nameof(Project))]
    public string ProjectId { get; set; } = null!;

    public Project Project { get; set; } = null!;

    [ForeignKey(nameof(Tag))]
    public string TagId { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}