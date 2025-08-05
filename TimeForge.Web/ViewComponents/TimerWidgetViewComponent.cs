using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.Web.ViewComponents;

public class TimerWidgetViewComponent : ViewComponent
{
    private readonly ITimeEntryService timeEntryService;

    public TimerWidgetViewComponent(ITimeEntryService timeEntryService)
    {
        this.timeEntryService = timeEntryService;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var runningEntry = await this.timeEntryService.GetCurrentRunningTimeEntryByUserIdAsync(userId);
        
        if (runningEntry is null)
            return View(new NavbarTimerViewModel());

        var viewModel = new NavbarTimerViewModel()
        {
            Id = runningEntry.Id,
            DurationInMiliseconds = (long)runningEntry.Duration.TotalMilliseconds,
            State = runningEntry.State,
            TaskName = runningEntry.TaskName
        };
        
        return View(viewModel);
    }
}