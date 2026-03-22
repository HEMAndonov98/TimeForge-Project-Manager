namespace TimeForge.Api.Features.Projects;

public class ProjectTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Todo", "InProgress", "Done"
}
