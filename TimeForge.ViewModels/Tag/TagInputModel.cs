using System.ComponentModel.DataAnnotations;
using TimeForge.Common.Dto_Validation;

namespace TimeForge.ViewModels.Tag;

public class TagInputModel
{
    [Required(ErrorMessage = TagDtoValidationConstants.TagNameRequiredErrorMessage)]
    [StringLength(TagDtoValidationConstants.TagNameMaxLength,
        MinimumLength = TagDtoValidationConstants.TagNameMinLength,
        ErrorMessage = TagDtoValidationConstants.TagNameLengthErrorMessage)]
    public string Name { get; set; } = null!;
    
    public string UserId { get; set; } = null!;
}