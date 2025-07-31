using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;

namespace TimeForge.Web.Areas.Manager.Controllers;

[Area("Manager")]
public class ManagerController : Controller
{
    private readonly IProjectService projectService;
    private readonly ITaskService taskService;


    public ManagerController(IProjectService projectService,
        ITaskService taskService)
    {
        this.projectService = projectService;
        this.taskService = taskService;
    }
    
    
    // GET
    public async Task<IActionResult> Index()
    {
        var user = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var projects = await this.projectService.GetAllProjectsAsync(user);
        
        ViewData["ProjectsCount"] = projects.Count();
        int completedTasksCount = 0;
        int totalTasksCount = 0;
        
        foreach (var project in projects)
        {
            var projectTasks = await this.taskService.GetTasksByProjectIdAsync(project.Id);
            
            completedTasksCount += projectTasks.Count(t => t.IsCompleted);
            totalTasksCount += projectTasks.Count();
        }
        
        ViewData["CompletionTaskPercentage"] = (completedTasksCount / (double)totalTasksCount).ToString("P");
        ViewData["TaskCompletionCount"] = completedTasksCount;
        
        return View();
    }
}