using System.Globalization;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;


namespace TimeForge.Web.Controllers;
/// <summary>
/// Handles project-related operations such as create, edit, details, and delete.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService projectService;
    private readonly ITaskService taskService;
    private readonly ILogger<ProjectController> logger;

    public ProjectController(IProjectService projectService, ITaskService taskService, ILogger<ProjectController> logger)
    {
        this.projectService = projectService;
        this.taskService = taskService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a paged list of projects for the current user.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>A paged result of projects.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedProjectViewModel>> Index(int page = 1)
    {
        try
        {
            var userId = this.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            int pageSize = 4;
            int projectsCount = await projectService.GetProjectsCountAsync(userId);
            int totalPages = (int)Math.Ceiling(projectsCount / (double)pageSize);

            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var projects = await this.projectService.GetAllProjectsAsync(userId, page, pageSize);
            var projectsList = projects.ToList();

            foreach (var project in projectsList)
            {
                project.UserId = userId;
                project.Tasks = (await this.taskService.GetTasksByProjectIdAsync(project.Id)).ToList();
            }

            var viewModel = new PagedProjectViewModel
            {
                Projects = projectsList,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return Ok(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets upcoming projects for the next 7 days.
    /// </summary>
    /// <returns>A dictionary of upcoming projects grouped by day.</returns>
    [HttpGet("upcoming")]
    public async Task<ActionResult<Dictionary<string, List<ProjectViewModel>>>> Upcoming()
    {
        try
        {
            string? userId = this.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var projects = await this.projectService.GetAllProjectsAsync(userId);
            var startDate = DateTime.UtcNow.Date;
            var days = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i))
                .ToList();

            var grouped = new Dictionary<string, List<ProjectViewModel>>();

            foreach (var date in days)
            {
                string dayName = date.ToString("dddd");
                string key = date == DateTime.Today.Date ? $"{dayName} (Today)" : dayName;

                var projectsOnDay = projects.Where(p =>
                    DateTime.TryParse(p.DueDate, out var due) &&
                    due.Date == date.Date)
                    .ToList();

                grouped[key] = projectsOnDay;
            }

            return Ok(grouped);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving upcoming projects");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="inputModel">The project data.</param>
    /// <returns>The created project (ID only for now based on service).</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProjectInputModel inputModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            inputModel.UserId = this.GetUserId()!;
            await this.projectService.CreateProjectAsync(inputModel);
            return Ok(new { message = "Project created successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating project");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets project details by ID.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>The project details.</returns>
    [HttpGet("{projectId}")]
    public async Task<ActionResult<ProjectViewModel>> Details(string projectId)
    {
        try
        {
            var viewModel = await this.projectService.GetProjectByIdAsync(projectId);
            var tasks = await this.taskService.GetTasksByProjectIdAsync(projectId);
            viewModel.Tasks = tasks.ToList();

            return Ok(viewModel);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving project details for {ProjectId}", projectId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="inputModel">The updated project data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{projectId}")]
    public async Task<IActionResult> Edit(string projectId, [FromBody] ProjectInputModel inputModel)
    {
        if (!ModelState.IsValid || projectId != inputModel.Id)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await this.projectService.UpdateProject(inputModel);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating project {ProjectId}", projectId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{projectId}")]
    public async Task<IActionResult> Delete(string projectId)
    {
        try
        {
            await this.projectService.DeleteProject(projectId);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets projects for the calendar view.
    /// </summary>
    /// <returns>A list of projects.</returns>
    [HttpGet("calendar")]
    public async Task<ActionResult<IEnumerable<ProjectViewModel>>> Calendar()
    {
        try
        {
            string? userId = this.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var projects = await this.projectService.GetAllProjectsAsync(userId);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving calendar projects");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates a project's due date.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="inputModel">The project data with new due date.</param>
    /// <returns>Success status.</returns>
    [HttpPost("{projectId}/due-date")]
    public async Task<IActionResult> UpdateDueDate(string projectId, [FromBody] ProjectInputModel inputModel)
    {
        try
        {
            if (!ModelState.IsValid || projectId != inputModel.Id)
            {
                return BadRequest(ModelState);
            }

            // Note: Keeping the original increment logic if it was intended for timezone/adjustment
            if (inputModel.DueDate.HasValue)
            {
                inputModel.DueDate = inputModel.DueDate.Value.AddDays(1);
            }
            
            await this.projectService.UpdateProject(inputModel);
            return Ok(new { success = true });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating due date for project {ProjectId}", projectId);
            return StatusCode(500, "Internal server error");
        }
    }

    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}