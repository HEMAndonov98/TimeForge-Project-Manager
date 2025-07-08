using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class Tag : BaseDeletableModel<string>
{
    public Tag()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    
    [Required]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(CreatedBy))]
    public string UserId { get; set; } = null!;

    public User CreatedBy { get; set; } = null!;

    [InverseProperty(nameof(ProjectTag.Tag))]
    public virtual List<ProjectTag> ProjectTags { get; set; } = new();
}