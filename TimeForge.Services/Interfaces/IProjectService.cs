using TimeForge.ViewModels.Project;

namespace TimeForge.Services.Interfaces;

public interface IProjectService
{
    Task CreateProjectAsync(ProjectInputModel inputModel);

    Task<ProjectViewModel> GetProjectByIdAsync(string projectId);

    Task<IEnumerable<ProjectViewModel>> GetAllProjectsAsync(string userId);

    void DeleteProject(string projectId);

    Task UpdateProject(ProjectInputModel inputModel);

    Task AddTaskToProject(string projectId, string taskId);

    Task RemoveTaskFromProject(string projectId, string taskId);

    Task AddTagToProject(string projectId, string tagId);
}