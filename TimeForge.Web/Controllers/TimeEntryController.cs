using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;

namespace TimeForge.Web.Controllers;
[Authorize]
public class TimeEntryController : Controller
{
    private readonly ITimeEntryService timeEntryService;
    private readonly ILogger<TimeEntryController> logger;

    public TimeEntryController(ITimeEntryService timeEntryService,
        ILogger<TimeEntryController> logger)
    {
        this.timeEntryService = timeEntryService;
        this.logger = logger;
    }

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

            var runningTimeEntry = await this.timeEntryService.GetCurrentRunningTimeEntryByUserIdAsync(userId);
            if (runningTimeEntry != null)
            {
                this.logger.LogInformation("Stopping running time entry {RunningTimeEntryId}", runningTimeEntry.Id);
                await this.timeEntryService.StopEntryAsync(runningTimeEntry.Id);
            }
            
            await this.timeEntryService.StartEntryAsync(taskId, userId);
            this.logger.LogInformation("Time entry started for task {TaskId}", taskId);
            return RedirectToAction("Index", "Home");
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
        return Ok();
    }

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
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(string entryId)
    {
        try
        {
            if (String.IsNullOrEmpty(entryId))
                throw new ArgumentNullException();

            await this.timeEntryService.ResumeEntryAsync(entryId);
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


    private string? GetUserId()
    => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}