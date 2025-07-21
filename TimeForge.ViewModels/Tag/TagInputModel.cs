using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TimeForge.ViewModels.Tag;

public class TagInputModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    
    [BindNever]
    [ValidateNever]
    public string UserId { get; set; } = null!;
}