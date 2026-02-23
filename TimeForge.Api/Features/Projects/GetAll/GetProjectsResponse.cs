namespace TimeForge.Api.Features.Projects.GetAll;

public class GetProjectsResponse
{
    public List<ProjectSummaryDto> Projects { get; set; } = new();
}

public class ProjectSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int TasksDone { get; set; }
    public int TasksTotal { get; set; }
}
