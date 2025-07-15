using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Web.Controllers;
[Authorize]
public class ProjectController : Controller
{
    private readonly IProjectService projectService;
    private readonly ITagService tagService;
    private readonly ILogger<ProjectController> logger;

    public ProjectController(IProjectService projectService, ITagService tagService, ILogger<ProjectController> logger)
    {
        this.projectService = projectService;
        this.tagService = tagService;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        //TODO Update to use userManager in the future for security
        string? userId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tags = await this.tagService.GetAllTagsAsync(userId);
        var inputModel = new ProjectInputModel()
        {
            UserId = userId!,
            Tags = tags.ToList()
        };
        return View(inputModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectInputModel inputModel)
    {
        if (!ModelState.IsValid)
        {
            return View(inputModel);
        }

        try
        {
            await this.projectService.CreateProjectAsync(inputModel);
            foreach (TagViewModel tag in inputModel.Tags)
            {
                await this.projectService.AddTagToProject(inputModel.Id, tag.Id);
            }
            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<IActionResult> Details(string projectId)
    {
        var viewModel = await this.projectService.GetProjectByIdAsync(projectId);

        return View(viewModel);
    }
}