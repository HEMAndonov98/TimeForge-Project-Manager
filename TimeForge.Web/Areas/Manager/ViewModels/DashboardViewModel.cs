using TimeForge.ViewModels.Project;

namespace TimeForge.Web.Areas.Manager.ViewModels;

public class DashboardViewModel
{
    public int ManagerProjectsCount { get; set; }

    public string? TaskCompletionPercentage { get; set; }

    //Map only project name and due date
    public IEnumerable<ProjectViewModel>? DueProjects { get; set; }
    
    
    //Chart username: [tasks]
    public Dictionary<string, string>? UsersTasksCompletionPercentage { get; set; }
    
    //Chart username: [projects]
    public Dictionary<string, int>? UsersProjects { get; set; }

    public int ManagedUsersCount { get; set; }
}