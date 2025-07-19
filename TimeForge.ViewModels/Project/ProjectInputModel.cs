using System.ComponentModel.DataAnnotations;
using TimeForge.Common.Constants;
using TimeForge.Common.GlobalErrorMessages;
using TimeForge.ViewModels.Tag;

namespace TimeForge.ViewModels.Project;

public class ProjectInputModel
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    [StringLength(ProjectValidationConstants.NameMaxLength,
        MinimumLength = ProjectValidationConstants.NameMinLength,
        ErrorMessage = (ProjectErrorMessages.ProjectNameLength))]
    public string Name { get; set; }

    public bool IsPublic { get; set; }

    public DateOnly? DueDate { get; set; }

    public string UserId { get; set; }
    
    public List<TagViewModel> Tags { get; set; }
}