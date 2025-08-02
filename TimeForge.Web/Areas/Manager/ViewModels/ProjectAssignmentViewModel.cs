using Microsoft.AspNetCore.Mvc.Rendering;
using TimeForge.ViewModels.Project;

namespace TimeForge.Web.Areas.Manager.ViewModels;

public class ProjectAssignmentViewModel
{

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public List<ProjectViewModel> Projects { get; set; }

    public List<SelectListItem> ManagedUsers { get; set; }
}