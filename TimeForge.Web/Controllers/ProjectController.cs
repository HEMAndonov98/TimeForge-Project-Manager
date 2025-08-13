using System.Globalization;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Web.Controllers;
/// <summary>
/// Handles project-related operations such as create, edit, details, and delete.
/// </summary>
[Authorize]
public class ProjectController : Controller
{
    private readonly IProjectService projectService;
    private readonly ITagService tagService;
    private readonly ITaskService taskService;
    private readonly ILogger<ProjectController> logger;

/// <summary>
/// Initializes a new instance of the <see cref="ProjectController"/>.
/// </summary>
/// <param name="projectService">Project service for project operations.</param>
/// <param name="tagService">Tag service for tag operations.</param>
/// <param name="taskService">Task service for task operations.</param>
/// <param name="logger">Logger instance.</param>
public ProjectController(IProjectService projectService, ITagService tagService, ITaskService taskService, ILogger<ProjectController> logger)
    {
        this.projectService = projectService;
        this.tagService = tagService;
        this.taskService = taskService;
        this.logger = logger;
    }


    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, List<string>? selectedTags = null)
    {
        try
        {
            int totalPages = 0;
            List<ProjectViewModel> projectsList = new List<ProjectViewModel>();

            var user = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            this.ViewData["IsLoggedIn"] = isLoggedIn;
            
            if (!string.IsNullOrEmpty(user))
            {
                int pageSize = 4;
                int projectsCount = 0;

                if (selectedTags != null && selectedTags.Any())
                {
                    projectsCount = await projectService.GetProjectsCountAsync(user, selectedTags);
                }
                else
                {
                    projectsCount = await projectService.GetProjectsCountAsync(user);
                    
                }
                totalPages = (int)Math.Ceiling(projectsCount / (double)pageSize);

                if (totalPages < 1)
                {
                    totalPages = 1;
                }

                if (page < 1)
                {
                    page = 1;
                }

                if (page > totalPages)
                {
                    page = totalPages;
                }

                IEnumerable<ProjectViewModel> projects = null;

                if (selectedTags != null && selectedTags.Any())
                {
                    projects = await this.projectService.GetAllProjectsAsync(user, page, pageSize, selectedTags);
                }
                else
                {
                    projects = await this.projectService.GetAllProjectsAsync(user, page, pageSize);
                }
                
                foreach (ProjectViewModel project in projects)
                {
                    project.UserId = user;
                    string projectId = project.Id;
                    project.Tasks = (await this.taskService.GetTasksByProjectIdAsync(projectId)).ToList();
                }

                projectsList = projects.ToList();
            }

            var viewModel = new PagedProjectViewModel()
            {
                Projects = projectsList,
                CurrentPage = page,
                TotalPages = totalPages,
                SelectedTags = selectedTags ?? new List<string>()
            };
            return View("Index", viewModel);
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
    public async Task<IActionResult> Upcoming()
    {
        string? userId = this.GetUserId();
        
        if (String.IsNullOrEmpty(userId))
            throw new ArgumentNullException();
        var projects = await this.projectService.GetAllProjectsAsync(userId);
        
        
        var startDate = DateTime.UtcNow.Date;
        var days = Enumerable.Range(1, 7)
            .Select(i => startDate.AddDays(i))
            .ToList();

        Dictionary<string, List<ProjectViewModel>> grouped = new Dictionary<string, List<ProjectViewModel>>();

        for (int i = 1; i <= days.Count; i++)
        {
            string day = days[i - 1].ToString("dddd");
            bool isToday = DateTime.Compare(days[i - 1], DateTime.Today.Date) == 0;
            string key = day;

            if (isToday)
            {
                key = $"{day} (Today)";
            }

            List<ProjectViewModel> projectsOnDay = projects.Where(p => 
                    DateTime.TryParse(p.DueDate, out var due) &&
                    DateTime.Compare(due.Date, days[i - 1].Date) == 0)
                .ToList();
            
            grouped[key] = projectsOnDay;
        }
        
        return View(grouped);
    }

/// <summary>
/// Displays the create project form.
/// </summary>
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


/// <summary>
/// Handles project creation form submission.
/// </summary>
/// <param name="inputModel">The project input model.</param>
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
/// <summary>
/// Displays project details by project ID.
/// </summary>
/// <param name="projectId">The project ID.</param>
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

/// <summary>
/// Displays the edit project form.
/// </summary>
/// <param name="projectId">The project ID.</param>
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

/// <summary>
/// Handles project edit form submission.
/// </summary>
/// <param name="inputModel">The project input model.</param>
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


/// <summary>
/// Displays the delete project confirmation partial view.
/// </summary>
/// <param name="projectId">The project ID.</param>
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

/// <summary>
/// Handles project deletion.
/// </summary>
/// <param name="viewModel">The project view model.</param>
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

/// <summary>
/// Displays the project calendar.
/// </summary>
/// <returns>The calendar view.</returns>
[HttpGet]
    public async Task<IActionResult> Calendar()
    {
        try
        {
            string? userId = this.GetUserId();
            if (String.IsNullOrEmpty(userId))
                throw new ArgumentNullException();
            var projects = await this.projectService
                .GetAllProjectsAsync(userId);

            return View(projects);
        }
        catch (ArgumentNullException)
        {
            return BadRequest($"Required parameter is null: GET/{nameof(ProjectController)}/{nameof(Calendar)}");
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDueDate(ProjectInputModel inputModel)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ArgumentNullException();

            inputModel.DueDate = inputModel.DueDate!.Value.AddDays(1);
            
            await this.projectService.UpdateProject(inputModel);

            return Ok(
                new
                {
                    success = true,
                });
        }
        catch (ArgumentNullException)
        {
            return BadRequest($"Required parameter is null : POST/{nameof(ProjectController)}/{nameof(UpdateDueDate)}");
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

    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}