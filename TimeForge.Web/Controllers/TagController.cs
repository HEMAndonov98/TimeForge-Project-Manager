using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Web.Controllers;
[Authorize]
public class TagController : Controller
{
    private readonly ITagService tagService;
    private readonly IProjectService projectService;

    private readonly ILogger<TagController> logger;

    public TagController(ITagService tagService, IProjectService projectService, ILogger<TagController> logger)
    {
        this.projectService = projectService;
        this.tagService = tagService;
        this.logger = logger;
        
        this.logger.LogInformation("Initializing TagController with tagService and projectService");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TagInputModel inputModel)
    {
        //TODO Make tags temporary before creating a new Project
        if (!ModelState.IsValid)
        {
            this.logger.LogInformation("Model state is invalid");
            return ViewComponent("CreateTag", new { projectId = inputModel.ProjectId });
        }

        try
        {
            await this.tagService.CreateTagAsync(inputModel);
            this.logger.LogInformation("Tag created successfully for project {ProjectId}", inputModel.ProjectId);

            return RedirectToAction("Create", "Project");
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error creating tag for project {ProjectId}", inputModel.ProjectId);
            ModelState.AddModelError(string.Empty, "Error creating tag. Please try again.");
            return ViewComponent("CreateTag", new { projectId = inputModel.ProjectId });
        }
    }
}