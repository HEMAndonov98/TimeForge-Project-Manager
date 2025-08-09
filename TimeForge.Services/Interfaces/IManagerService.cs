namespace TimeForge.Services.Interfaces;

public interface IManagerService
{
    public Task AssignProjectToUserAsync(string projectId, string userId);
}