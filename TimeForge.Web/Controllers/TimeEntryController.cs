using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TimeForge.Services.Interfaces;

namespace TimeForge.Web.Controllers;
/// <summary>
/// Handles time entry operations for tasks (start, pause, resume, stop).
/// </summary>
[Authorize]
public class TimeEntryController : Controller
{
    private readonly ITimeEntryService timeEntryService;
    private readonly ILogger<TimeEntryController> logger;

/// <summary>
/// Initializes a new instance of the <see cref="TimeEntryController"/>.
/// </summary>
/// <param name="timeEntryService">Time entry service for time tracking.</param>
/// <param name="logger">Logger instance.</param>
public TimeEntryController(ITimeEntryService timeEntryService,
        ILogger<TimeEntryController> logger)
    {
        this.timeEntryService = timeEntryService;
        this.logger = logger;
    }


/// <summary>
/// Starts a time entry for the specified task.
/// </summary>
/// <param name="taskId">The ID of the task.</param>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Start(string taskId)
    {
        try
        {
            this.logger.LogInformation("Starting time entry for task {TaskId}", taskId);
            var userId = this.GetUserId();
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(taskId))
                throw new ArgumentNullException();
            
            await this.timeEntryService.StartEntryAsync(taskId, userId);
            this.logger.LogInformation("Time entry started for task {TaskId}", taskId);
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
        return RedirectToAction("Index", "Home");
    }


/// <summary>
/// Pauses a time entry.
/// </summary>
/// <param name="entryId">The time entry ID.</param>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Pause(string entryId)
    {
        try
        {
            if (String.IsNullOrEmpty(entryId))
                throw new ArgumentNullException();
            
            await this.timeEntryService.PauseEntryAsync(entryId);
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

        return RedirectToAction("Index", "Home");
    }
    

/// <summary>
/// Resumes a paused time entry.
/// </summary>
/// <param name="entryId">The time entry ID.</param>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Resume(string entryId)
    {
        try
        {
            string? userId = this.GetUserId();
            if (String.IsNullOrEmpty(entryId) || String.IsNullOrEmpty(userId))
                throw new ArgumentNullException();
            

            await this.timeEntryService.ResumeEntryAsync(entryId, userId);
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
        return RedirectToAction("Index", "Home");
    }

/// <summary>
/// Stops a time entry.
/// </summary>
/// <param name="entryId">The time entry ID.</param>
public async Task<IActionResult> Stop(string entryId)
    {
        try
        {
            if (String.IsNullOrEmpty(entryId))
                throw new ArgumentNullException();
            await this.timeEntryService.StopEntryAsync(entryId);
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

        return RedirectToAction("Index", "Home");
    }


/// <summary>
/// Gets the current user's ID from claims.
/// </summary>
/// <returns>The user ID as a string, or null if not found.</returns>
private string? GetUserId()
    => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}