using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.List;

namespace TimeForge.Web.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskCollectionController(ITaskCollectionService taskCollectionService, ILogger<TaskCollectionController> logger) : ControllerBase
{
    private readonly ITaskCollectionService taskCollectionService = taskCollectionService;
    private readonly ILogger<TaskCollectionController> logger = logger;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskCollectionViewModel>>> Index()
    {
        try
        {
            string userId = this.GetUserId();
            var lists = await this.taskCollectionService.GetAllTaskCollectionsAsync(userId);
            return Ok(lists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting task collections");
            return StatusCode(500, "An error occurred while retrieving task collections.");
        }
    }
    
    
    [HttpPost]
    public async Task<ActionResult<TaskCollectionViewModel>> Create(TaskCollectionInputModel inputModel)
    {
        try
        {
            inputModel.UserId = this.GetUserId();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await this.taskCollectionService.CreateTaskCollectionAsync(inputModel);
            // Re-fetch or return based on what the service typically does.
            // Service returns Task, so we might need to return the list or just Ok.
            return Ok(inputModel); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating task collection");
            return StatusCode(500, "An error occurred while creating the task collection.");
        }
    }

    [HttpPost("item")]
    public async Task<IActionResult> AddTaskItem(TaskItemInputModel inputModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await this.taskCollectionService.CreateTaskItemAsync(inputModel);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding task item");
            return StatusCode(500, "An error occurred while adding the task item.");
        }
    }

    [HttpDelete("{taskCollectionId}")]
    public async Task<IActionResult> Delete(string taskCollectionId)
    {
        try
        {
            await this.taskCollectionService.DeleteTaskCollection(taskCollectionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting task collection {Id}", taskCollectionId);
            return NotFound();
        }
    }

    [HttpPost("item/{taskId}/complete")]
    public async Task<IActionResult> CompleteTaskItem(string taskId)
    {
        try
        {
            // TODO: Implement CompleteTaskItem in service
            // await this.taskCollectionService.CompleteTaskItemAsync(taskId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing task item {Id}", taskId);
            return StatusCode(500, "An error occurred while completing the task item.");
        }
    }
    
    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
}
