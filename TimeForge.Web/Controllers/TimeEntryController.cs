using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;

namespace TimeForge.Web.Controllers;
/// <summary>
/// Handles time entry operations for tasks (start, pause, resume, stop).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntryController : ControllerBase
{
    private readonly ITimeEntryService timeEntryService;
    private readonly ILogger<TimeEntryController> logger;

    public TimeEntryController(ITimeEntryService timeEntryService, ILogger<TimeEntryController> logger)
    {
        this.timeEntryService = timeEntryService;
        this.logger = logger;
    }

    /// <summary>
    /// Starts a time entry for the specified task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromQuery] string taskId)
    {
        try
        {
            this.logger.LogInformation("Starting time entry for task {TaskId}", taskId);
            var userId = this.GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(taskId))
            {
                return BadRequest("Required parameter is null");
            }
            
            await this.timeEntryService.StartEntryAsync(taskId, userId);
            return Ok(new { message = "Time entry started" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error starting time entry");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Pauses a time entry.
    /// </summary>
    /// <param name="entryId">The time entry ID.</param>
    [HttpPost("pause")]
    public async Task<IActionResult> Pause([FromQuery] string entryId)
    {
        try
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return BadRequest("Required parameter is null");
            }
            
            await this.timeEntryService.PauseEntryAsync(entryId);
            return Ok(new { message = "Time entry paused" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error pausing time entry");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Resumes a paused time entry.
    /// </summary>
    /// <param name="entryId">The time entry ID.</param>
    [HttpPost("resume")]
    public async Task<IActionResult> Resume([FromQuery] string entryId)
    {
        try
        {
            string? userId = this.GetUserId();
            if (string.IsNullOrEmpty(entryId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("Required parameter is null");
            }

            await this.timeEntryService.ResumeEntryAsync(entryId, userId);
            return Ok(new { message = "Time entry resumed" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error resuming time entry");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Stops a time entry.
    /// </summary>
    /// <param name="entryId">The time entry ID.</param>
    [HttpPost("stop")]
    public async Task<IActionResult> Stop([FromQuery] string entryId)
    {
        try
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return BadRequest("Required parameter is null");
            }
            await this.timeEntryService.StopEntryAsync(entryId);
            return Ok(new { message = "Time entry stopped" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error stopping time entry");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    /// <returns>The user ID as a string, or null if not found.</returns>
    private string? GetUserId()
        => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}