using System.ComponentModel.DataAnnotations;
using TimeForge.Common.Dto_Validation;

namespace TimeForge.ViewModels.Tag;

public class TagInputModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required(ErrorMessage = TagDtoValidationConstants.TagNameRequiredErrorMessage)]
    [StringLength(TagDtoValidationConstants.TagNameMaxLength,
        MinimumLength = TagDtoValidationConstants.TagNameMinLength,
        ErrorMessage = TagDtoValidationConstants.TagNameLengthErrorMessage)]
    public string Name { get; set; } = null!;
    
    public string UserId { get; set; } = null!;

    public string ProjectId { get; set; } = null!;
}