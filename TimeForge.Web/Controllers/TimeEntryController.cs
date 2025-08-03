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
            var userId = this.GetUserId();
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(taskId))
                throw new ArgumentNullException();
            
            await this.timeEntryService.StartEntryAsync(taskId, userId!);
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
        return Ok();
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
        return Ok();
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

        return Ok();
    }


    private string? GetUserId()
    => this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}