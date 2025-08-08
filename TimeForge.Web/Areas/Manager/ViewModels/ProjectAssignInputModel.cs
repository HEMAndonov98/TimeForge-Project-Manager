using System.ComponentModel.DataAnnotations;

namespace TimeForge.Web.Areas.Manager.ViewModels;

public class ProjectAssignInputModel
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string ProjectId { get; set; }
}