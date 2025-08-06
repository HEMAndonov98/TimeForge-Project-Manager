using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.Services.Interfaces;

public interface ITimeEntryService
{
    public Task StartEntryAsync(string taskId, string userId);
    
    public Task ResumeEntryAsync(string entryId);
    
    public Task PauseEntryAsync(string entryId);
    
    public Task StopEntryAsync(string entryId);

    public Task<TimeEntryViewModel?> GetCurrentRunningTimeEntryByUserIdAsync(string userId);
    
    public Task<TimeEntryViewModel?> GetCurrentPausedTimeEntryByUserIdAsync(string userId);
}