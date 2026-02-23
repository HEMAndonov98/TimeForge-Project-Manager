using FastEndpoints;
using FluentValidation;

namespace TimeForge.Api.Features.Projects.Create;

public class CreateProjectValidator : Validator<CreateProjectRequest>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(100).WithMessage("Project name is too long");
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description is too long");
    }
}
