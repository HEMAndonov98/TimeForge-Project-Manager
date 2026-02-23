using FastEndpoints;
using FluentValidation;

namespace TimeForge.Api.Features.Tasks.UpdateStatus;

public class UpdateTaskStatusValidator : Validator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("A valid status is required.");
    }
}
