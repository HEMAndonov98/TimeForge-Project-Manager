using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.ViewModels.Shared;

namespace TimeForge.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProjectService projectService;
    private readonly ITaskService taskService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IProjectService projectService, ITaskService taskService)
    {
        _logger = logger;
        this.taskService = taskService;
        this.projectService = projectService;
    }

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