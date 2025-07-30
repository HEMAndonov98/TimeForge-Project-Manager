namespace TimeForge.Services.Interfaces;

public interface ITimeEntryService
{
    public Task StartEntryAsync(string taskId, string userId);
    
    public Task ResumeEntryAsync(string entryId);
    
    public Task PauseEntryAsync(string entryId);
    
    public Task StopEntryAsync(string entryId);
}