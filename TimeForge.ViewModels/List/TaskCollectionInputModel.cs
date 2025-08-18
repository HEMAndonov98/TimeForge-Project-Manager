using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using TimeForge.Common.Constants;
using TimeForge.Common.Dto_Validation;

namespace TimeForge.ViewModels.List;

public class TaskCollectionInputModel
{
    [Required(ErrorMessage = TaskCollectionDtoMessages.TaskCollectionNameRequired)]
    [StringLength(TaskCollectionConstants.TaskCollectionNameMaxLength,
        MinimumLength = TaskCollectionConstants.TaskCollectionNameMinLength,
        ErrorMessage = TaskCollectionDtoMessages.TaskCollectionNameLength)]
    public string Title { get; set; }

    [ValidateNever]
    public string UserId { get; set; } = null!;
}