using System.Globalization;
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
    private readonly ITaskService taskService;
    private readonly ILogger<ProjectController> logger;

    public ProjectController(IProjectService projectService, ITagService tagService, ITaskService taskService, ILogger<ProjectController> logger)
    {
        this.projectService = projectService;
        this.tagService = tagService;
        this.taskService = taskService;
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
            Tags = tags
                .Select(t => new TagInputModel()
                {
                    Id = t.Id,
                    Name = t.Name,
                    UserId = userId
                })
                .ToList()
        };
        return View(inputModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectInputModel inputModel)
    {
        foreach (TagInputModel tag in inputModel.Tags)
        {
            tag.UserId = inputModel.UserId;
        }
        
        if (!ModelState.IsValid)
        {
            return View(inputModel);
        }

        try
        {
            await this.projectService.CreateProjectAsync(inputModel);
            foreach (TagInputModel tag in inputModel.Tags)
            {
                await this.tagService.CreateTagAsync(tag);
                await this.projectService.AddTagToProject(inputModel.Id, tag.Id);
            }
            return RedirectToAction("Index", "Home");
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
    [HttpGet]
    public async Task<IActionResult> Details(string projectId)
    {
        try
        {
            var viewModel = await this.projectService.GetProjectByIdAsync(projectId);
            var tasks = await this.taskService.GetTasksByProjectIdAsync(projectId);
            ViewData["UserId"] = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            viewModel.Tasks = tasks.ToList();

            return View(viewModel);
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string projectId)
    {
        try
        {
            var viewModel = await this.projectService.GetProjectByIdAsync(projectId);
            var inputModel = new ProjectInputModel()
            {
                Id = viewModel.Id,
                UserId = viewModel.UserId,
                DueDate = DateOnly.TryParseExact(viewModel.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate) ? dueDate : null,
                Name = viewModel.Name,
                IsPublic = viewModel.IsPublic,
                Tags = viewModel.Tags
                    .Select(t => new TagInputModel() { Id = t.Id, Name = t.Name})
                .ToList()
            };

            return View(inputModel);
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectInputModel inputModel)
    {
        if (!ModelState.IsValid)
        {
            return View(inputModel);
        }

        try
        {
            await this.projectService.UpdateProject(inputModel);

            var previousTags = await this.tagService.GetAllTagsByProjectIdAsync(inputModel.Id);

            var currentTags = inputModel.Tags;
            
            //Add new tags to project
            foreach (var currentTag in currentTags)
            {
                if (!previousTags.Any(t => t.Name == currentTag.Name))
                {
                    currentTag.UserId = inputModel.UserId;
                    await this.tagService.CreateTagAsync(currentTag);
                    await this.projectService.AddTagToProject(inputModel.Id, currentTag.Id);
                }
            }

            // Remove tags not in current input model
            foreach (var previousTag in previousTags)
            {
                if (!currentTags.Any(t => t.Name != previousTag.Name))
                {
                    await this.projectService.RemoveTagFromProjectAsync(inputModel.Id, previousTag.Id);
                }
            }

            return RedirectToAction("Details", "Project", new { projectId = inputModel.Id });
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            // Consider logging ex here
            return StatusCode(500);
        }
    }


    [HttpGet]
    public async Task<IActionResult> Delete(string projectId)
    {
        try
        {
            var viewModel = await this.projectService.GetProjectByIdAsync(projectId);
            return PartialView("Delete", viewModel);
        }
        catch (Exception e)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProject(ProjectViewModel viewModel)
    {
        try
        {
            await this.projectService.DeleteProject(viewModel.Id);
            return RedirectToAction("Index", "Home");
        }
        catch (ArgumentNullException)
        {
            return BadRequest("Required parameter is null");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}