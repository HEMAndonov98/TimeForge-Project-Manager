using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.ViewModels.Tag;

namespace TimeForge.Services;

public class ProjectService : IProjectService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly ILogger<ProjectService> logger;

    public ProjectService(ITimeForgeRepository timeForgeRepository, ILogger<ProjectService> logger)
    {
        this.timeForgeRepository = timeForgeRepository ?? throw new ArgumentNullException(nameof(timeForgeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CreateProjectAsync(ProjectInputModel inputModel)
    {
        try
        {
            if (inputModel == null)
            {
                this.logger.LogWarning("CreateProjectAsync was called with a null inputModel.");
                throw new ArgumentNullException(nameof(inputModel), "InputModel cannot be null");
            }

            var newProject = new Project()
            {
                Name = inputModel.Name,
                IsPublic = inputModel.IsPublic,
                DueDate = inputModel.DueDate,
                UserId = inputModel.UserId,
            };

            await this.timeForgeRepository.AddAsync(newProject);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully created a new project with Name: {Name}.", newProject.Name);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in CreateProjectAsync.");
            throw new InvalidOperationException("An error occurred while creating the project.", ex);
        }
    }

    public async Task<ProjectViewModel> GetProjectByIdAsync(string projectId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                this.logger.LogWarning("GetProjectByIdAsync was called with a null or empty projectId.");
                throw new ArgumentNullException(nameof(projectId), "ProjectId cannot be null or empty");
            }

            var project = await this.timeForgeRepository.All<Project>(e => e.Id == projectId)
                .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (project == null)
            {
                this.logger.LogWarning("Project with Id {ProjectId} not found in GetProjectByIdAsync.", projectId);
                throw new InvalidOperationException($"Project with id {projectId} not found.");
            }

            var tags = project.ProjectTags.Select(pt => new TagViewModel()
            {
                Id = pt.TagId,
                Name = pt.Tag.Name
            }).ToList();

            var viewModel = new ProjectViewModel()
            {
                Id = project.Id,
                Name = project.Name,
                DueDate = project.DueDate?.ToString("dd/MM/yyyy"),
                CreatedBy = project.CreatedBy.UserName ?? string.Empty,
                Tags = tags
            };

            this.logger.LogInformation("Successfully retrieved project with Id: {ProjectId}.", projectId);
            return viewModel;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in GetProjectByIdAsync while fetching project with Id: {ProjectId}.", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectViewModel>> GetAllProjectsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("GetAllProjectsAsync was called with a null or empty userId.");
                throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty");
            }

            // Fetch only what is needed from the database
            var projects = await this.timeForgeRepository.All<Project>(e => e.UserId == userId)
                .Include(p => p.ProjectTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.CreatedBy)
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.DueDate,
                    CreatedBy = p.CreatedBy.UserName,
                    Tags = p.ProjectTags.Select(pt => new { pt.Tag.Id, pt.Tag.Name }).ToList()
                })
                .ToListAsync();

            // Perform projection to ViewModel in memory
            var projectViewModels = projects.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DueDate = p.DueDate?.ToString("dd/MM/yyyy"), // Format handled in memory
                CreatedBy = p.CreatedBy ?? string.Empty,
                Tags = p.Tags.Select(tag => new TagViewModel
                {
                    Id = tag.Id,
                    Name = tag.Name
                }).ToList()
            }).ToList();

            this.logger.LogInformation("GetAllProjectsAsync successfully retrieved {ProjectCount} projects for userId {UserId}.", projectViewModels.Count, userId);
            return projectViewModels;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in GetAllProjectsAsync for userId {UserId}.", userId);
            throw new InvalidOperationException("An error occurred while retrieving projects", ex);
        }
    }

    public async void DeleteProject(string projectId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
            {
                this.logger.LogWarning("DeleteProject was called with a null or empty projectId.");
                throw new ArgumentNullException(nameof(projectId), "ProjectId cannot be null or empty");
            }

            var project = await this.timeForgeRepository.GetByIdAsync<Project>(projectId);

            if (project == null)
            {
                this.logger.LogWarning("Project with Id {ProjectId} not found in DeleteProject.", projectId);
                throw new ArgumentException($"Project with Id {projectId} not found.");
            }

            this.timeForgeRepository.Delete(projectId);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully deleted project with Id: {ProjectId}.", projectId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in DeleteProject for projectId: {ProjectId}.", projectId);
            throw;
        }
    }

    public async void UpdateProject(ProjectInputModel inputModel)
    {
        try
        {
            if (inputModel == null)
            {
                this.logger.LogWarning("UpdateProject was called with a null inputModel.");
                throw new ArgumentNullException(nameof(inputModel), "InputModel cannot be null");
            }

            var project = await this.timeForgeRepository.GetByIdAsync<Project>(inputModel.Id);

            if (project == null)
            {
                this.logger.LogWarning("Project with Id {Id} not found in UpdateProject.", inputModel.Id);
                throw new ArgumentException($"Project with Id {inputModel.Id} not found.");
            }

            this.timeForgeRepository.Update(project);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully updated project with Id: {Id}.", inputModel.Id);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in UpdateProject for projectId: {Id}.", inputModel?.Id);
            throw;
        }
    }

    public async Task AddTaskToProject(string projectId, string taskId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(taskId))
            {
                this.logger.LogWarning("AddTaskToProject was called with invalid parameters. ProjectId: {ProjectId}, TaskId: {TaskId}.", projectId, taskId);
                throw new ArgumentNullException(projectId, taskId);
            }

            var project = await this.timeForgeRepository.GetByIdAsync<Project>(projectId);
            var task = await this.timeForgeRepository.GetByIdAsync<ProjectTask>(taskId);

            if (project == null || task == null)
            {
                this.logger.LogWarning("Either project or task was not found in AddTaskToProject. ProjectId: {ProjectId}, TaskId: {TaskId}.", projectId, taskId);
                throw new ArgumentException("Both project and task must exist.");
            }

            task.ProjectId = projectId;
            this.timeForgeRepository.Update(task);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully added task with Id {TaskId} to project with Id {ProjectId}.", taskId, projectId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in AddTaskToProject for projectId: {ProjectId}, taskId: {TaskId}.", projectId, taskId);
            throw;
        }
    }

    public async Task RemoveTaskFromProject(string projectId, string taskId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(taskId))
            {
                this.logger.LogWarning("RemoveTaskFromProject was called with invalid parameters. ProjectId: {ProjectId}, TaskId: {TaskId}.", projectId, taskId);
                throw new ArgumentNullException("projectId");
            }

            var project = await this.timeForgeRepository.GetByIdAsync<Project>(projectId);
            var task = await this.timeForgeRepository.GetByIdAsync<ProjectTask>(taskId);

            if (project == null || task == null)
            {
                this.logger.LogWarning("Either project or task was not found in RemoveTaskFromProject. ProjectId: {ProjectId}, TaskId: {TaskId}.", projectId, taskId);
                throw new ArgumentException("Both project and task must exist.");
            }

            project.Tasks.Remove(task);
            this.timeForgeRepository.Update(project);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully removed task with Id {TaskId} from project with Id {ProjectId}.", taskId, projectId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in RemoveTaskFromProject for projectId: {ProjectId}, taskId: {TaskId}.", projectId, taskId);
            throw;
        }
    }

    public async Task AddTagToProject(string projectId, string tagId)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(tagId))
            {
                this.logger.LogWarning("AddTagToProject was called with invalid parameters. ProjectId: {ProjectId}, TagId: {TagId}.", projectId, tagId);
                throw new ArgumentNullException(projectId, tagId);
            }

            var project = await this.timeForgeRepository.GetByIdAsync<Project>(projectId);
            var tag = await this.timeForgeRepository.GetByIdAsync<Tag>(tagId);

            if (project == null || tag == null)
            {
                this.logger.LogWarning("Either project or tag was not found in AddTagToProject. ProjectId: {ProjectId}, TagId: {TagId}.", projectId, tagId);
                throw new ArgumentException("Both project and tag must exist.");
            }

            var newProjectTag = new ProjectTag()
            {
                ProjectId = projectId,
                TagId = tagId
            };

            await this.timeForgeRepository.AddAsync(newProjectTag);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully added tag with Id {TagId} to project with Id {ProjectId}.", tagId, projectId);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in AddTagToProject for projectId: {ProjectId}, tagId: {TagId}.", projectId, tagId);
            throw;
        }
    }
}