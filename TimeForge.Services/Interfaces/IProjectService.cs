using TimeForge.ViewModels.Project;

namespace TimeForge.Services.Interfaces;

public interface IProjectService
{
    Task CreateProjectAsync(ProjectInputModel inputModel);

    Task<ProjectViewModel> GetProjectByIdAsync(string projectId);

    Task<IEnumerable<ProjectViewModel>> GetAllProjectsAsync(string userId);
    Task<IEnumerable<ProjectViewModel>> GetAllProjectsAsync(string userId, int pageNumber, int pageSize);

    Task DeleteProject(string projectId);
    
    Task<int> GetProjectsCountAsync(string userId);

    Task UpdateProject(ProjectInputModel inputModel);

    Task AddTagToProject(string projectId, string tagId);
    
    Task RemoveTagFromProjectAsync(string projectId, string tagId);
}