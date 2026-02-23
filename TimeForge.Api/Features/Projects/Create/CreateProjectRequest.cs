namespace TimeForge.Api.Features.Projects.Create;

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Color { get; set; } = "blue";
}
