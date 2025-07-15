using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.ViewModels.Shared;

namespace TimeForge.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProjectService projectService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IProjectService projectService)
    {
        _logger = logger;
        this.projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        var user = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
        this.ViewData["IsLoggedIn"] = isLoggedIn;
        
        var projects = new List<ProjectViewModel>();
        if (!string.IsNullOrEmpty(user))
        {
            var data = await this.projectService.GetAllProjectsAsync(user);
            projects = data.ToList();
        }
        return View("Index", projects);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}