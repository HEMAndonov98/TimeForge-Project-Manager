using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;

namespace TimeForge.Web.Controllers;

public class TaskController : Controller
{
    
    private readonly ITaskService taskService;
    private readonly ILogger<TaskController> logger;

    public TaskController(ITaskService taskService, ILogger<TaskController> logger)
    {
        this.taskService = taskService;
        this.logger = logger;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskInputModel inputModel)
    {
        this.logger.LogInformation("Creating task for project {ProjectId}", inputModel.ProjectId);
        await this.taskService.CreateTaskAsync(inputModel);
        return RedirectToAction("Details", "Project", new { projectId = inputModel.ProjectId });
    }
}