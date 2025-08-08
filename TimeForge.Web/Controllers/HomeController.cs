using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;

namespace TimeForge.Web.Controllers;

/// <summary>
/// Handles home page and general navigation actions.
/// </summary>
public class HomeController : Controller
{
    private readonly IProjectService projectService;
    private readonly ITaskService taskService;
    private readonly ILogger<HomeController> _logger;

/// <summary>
/// Initializes a new instance of the <see cref="HomeController"/>.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="projectService">Project service for project operations.</param>
/// <param name="taskService">Task service for task operations.</param>
public HomeController(ILogger<HomeController> logger, IProjectService projectService, ITaskService taskService)
    {
        _logger = logger;
        this.taskService = taskService;
        this.projectService = projectService;
    }

/// <summary>
/// Displays the home page with a paged list of projects for the logged-in user.
/// </summary>
/// <param name="page">The page number to display.</param>
/// <returns>The home view with the project list.</returns>
[HttpGet]
public async Task<IActionResult> Index(int page = 1)
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

                int projectsCount = await projectService.GetProjectsCountAsync(user);
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
                
                
                IEnumerable<ProjectViewModel> projects = await this.projectService.GetAllProjectsAsync(user, page, pageSize);
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
                TotalPages = totalPages
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

/// <summary>
/// Displays the privacy policy page.
/// </summary>
[HttpGet]
public IActionResult Privacy()
    {
        return View();
    }

    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // public IActionResult Error()
    // {
    //     return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    // }
}