using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;

namespace TimeForge.Web.Controllers;
/// <summary>
/// Handles task-related actions such as create, complete, and retrieval.
/// </summary>
[Authorize]
public class TaskController : Controller
{
    
    private readonly ITaskService taskService;
    private readonly ILogger<TaskController> logger;

/// <summary>
/// Initializes a new instance of the <see cref="TaskController"/>.
/// </summary>
/// <param name="taskService">Task service for task operations.</param>
/// <param name="logger">Logger instance.</param>
public TaskController(ITaskService taskService, ILogger<TaskController> logger)
    {
        this.taskService = taskService;
        this.logger = logger;
    }
    

/// <summary>
/// Handles the creation of a new task for a project.
/// </summary>
/// <param name="listAndFormModel">The model containing task input data.</param>
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

        try
        {
            await this.taskService.CreateTaskAsync(inputModel);

            this.logger.LogInformation("Task added for project {ProjectId}", inputModel.ProjectId);
            return RedirectToAction("Details", "Project", new { projectId = inputModel.ProjectId });
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
/// Marks a task as complete.
/// </summary>
/// <param name="taskId">The ID of the task to complete.</param>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Complete(string taskId)
    {
        try
        {
            await this.taskService.CompleteTask(taskId);
            return Ok();
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
/// Retrieves the partial view of the task list for a project.
/// </summary>
/// <param name="projectId">The project ID.</param>
[HttpGet]
public async Task<IActionResult> GetTaskListPartial(string projectId)
    {
        try
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