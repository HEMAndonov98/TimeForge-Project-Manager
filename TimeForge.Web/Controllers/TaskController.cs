using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;

namespace TimeForge.Web.Controllers;

/// <summary>
/// Web API Controller for managing tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService taskService;
    private readonly ITimeEntryService timeEntryService;
    private readonly ILogger<TaskController> logger;

    public TaskController(
        ITaskService taskService,
        ITimeEntryService timeEntryService,
        ILogger<TaskController> logger)
    {
        this.taskService = taskService;
        this.timeEntryService = timeEntryService;
        this.logger = logger;
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="input">The task data.</param>
    /// <returns>The created task.</returns>
    [HttpPost]
    public async Task<ActionResult<TaskViewModel>> Create([FromBody] TaskInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await taskService.CreateTaskAsync(input);
            // Since CreateTaskAsync doesn't return the ID, return Ok.
            return Ok(); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating task");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Marks a task as complete.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{taskId}/complete")]
    public async Task<IActionResult> Complete(string taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var runningEntry = await timeEntryService.GetCurrentRunningTimeEntryByUserIdAsync(userId);
            if (runningEntry != null && runningEntry.TaskId == taskId)
            {
                await timeEntryService.StopEntryAsync(runningEntry.Id);
            }

            await taskService.CompleteTask(taskId);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing task {TaskId}", taskId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves all tasks for a specific project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <returns>A list of tasks.</returns>
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<TaskViewModel>>> GetByProject(string projectId)
    {
        try
        {
            var tasks = await taskService.GetTasksByProjectIdAsync(projectId);
            return Ok(tasks);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves a task by its ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>The task data.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskViewModel>> GetById(string id)
    {
        try
        {
            var task = await taskService.GetTaskByIdAsync(id);
            return Ok(task);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving task {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await taskService.DeleteTaskAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting task {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}