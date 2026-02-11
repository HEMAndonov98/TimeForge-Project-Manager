

namespace TimeForge.ViewModels.Tag;

public class TagInputModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    

    public string UserId { get; set; } = null!;
}