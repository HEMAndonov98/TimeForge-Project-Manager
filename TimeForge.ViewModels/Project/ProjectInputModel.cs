namespace TimeForge.ViewModels.Project;

public class ProjectInputModel
{
    public string Id { get; set; }
    public string Name { get; set; }

    public bool IsPublic { get; set; }

    public DateTime? DueDate { get; set; }

    public string UserId { get; set; }
}