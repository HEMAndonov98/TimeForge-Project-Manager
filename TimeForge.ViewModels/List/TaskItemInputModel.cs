using System.ComponentModel.DataAnnotations;
using TimeForge.Common.Constants;
using TimeForge.Common.Dto_Validation;

namespace TimeForge.ViewModels.List;

public class TaskItemInputModel
{
    [Required(ErrorMessage = TaskCollectionDtoMessages.TaskItemTitleRequired)]
    [StringLength(TaskCollectionConstants.TaskItemTitleMaxLength,
        MinimumLength = TaskCollectionConstants.TaskItemTitleMinLength,
        ErrorMessage = TaskCollectionDtoMessages.TaskItemTitleLength)]
    public string Title { get; set; } = null!;

    [Required]
    public string TaskCollectionId { get; set; } = null!;


}