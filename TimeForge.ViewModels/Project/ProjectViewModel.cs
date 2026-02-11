
using TimeForge.ViewModels.Task;

namespace TimeForge.ViewModels.Project;

public class ProjectViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    public string? DueDate { get; set; }

    public string CreatedBy { get; set; }

    public string UserId { get; set; }

    public string? AssignedToUserId { get; set; }

    public bool IsPublic { get; set; }
    
    public List<TaskViewModel> Tasks { get; set; }

}