using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.Services.Interfaces;

public interface ITimeEntryService
{
    public Task StartEntryAsync(string taskId, string userId);
    
    public Task ResumeEntryAsync(string entryId, string userId);
    
    public Task PauseEntryAsync(string entryId);
    
    public Task StopEntryAsync(string entryId);

    public Task<TimeEntryViewModel?> GetCurrentRunningTimeEntryByUserIdAsync(string userId);
    
    public Task<IEnumerable<TimeEntryViewModel>> GetCurrentPausedTimeEntryByUserIdAsync(string userId);
}