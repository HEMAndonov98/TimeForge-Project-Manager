using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class Project : BaseDeletableModel<string>
{
    public Project()
    {
        this.Id = Guid.NewGuid().ToString();
    }
    
    [Required]
    [MaxLength(ProjectValidationConstants.NameMaxLength)]
    public string Name { get; set; } = null!;

    public bool IsPublic { get; set; } = true;

    public DateOnly? DueDate { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public string UserId { get; set; } = null!;

    public User CreatedBy { get; set; } = null!;

    [InverseProperty(nameof(ProjectTask.Project))]
    public virtual List<ProjectTask> Tasks { get; set; } = new();

    [InverseProperty(nameof(ProjectTag.Project))]
    public virtual List<ProjectTag> ProjectTags { get; set; } = new();
}