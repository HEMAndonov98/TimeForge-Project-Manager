using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;

namespace TimeForge.Web.Controllers;
[Authorize]
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
    public async Task<IActionResult> Create(TaskListAndFormModel listAndFormModel)
    {
        var inputModel = listAndFormModel.TaskInputModel;
        
        this.logger.LogInformation("Creating task for project {ProjectId}", inputModel.ProjectId);

        if (!ModelState.IsValid)
        {
            this.logger.LogWarning("Model state is invalid for Task with description: {TaskDescription}", inputModel.Name);
            return RedirectToAction("Details", "Project", new { projectId = inputModel.ProjectId });
        }
        await this.taskService.CreateTaskAsync(inputModel);
        
        this.logger.LogInformation("Task added for project {ProjectId}", inputModel.ProjectId);
        return RedirectToAction("Details", "Project", new { projectId = inputModel.ProjectId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(string taskId)
    {
        await this.taskService.CompleteTask(taskId);

        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTaskListPartial(string projectId)
    {
        var model = await this.taskService.GetTasksByProjectIdAsync(projectId);
        var taskListAndFormModel = new TaskListAndFormModel()
        {
            Tasks = model.ToList(),
            TaskInputModel = new TaskInputModel(),
            ProjectId = projectId
        };
        return PartialView("_ProjectTaskList", taskListAndFormModel);
    }

}