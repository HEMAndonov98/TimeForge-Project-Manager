using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;

using TimeForge.ViewModels.Task;

namespace TimeForge.Services;

/// <summary>
/// Service for managing projects, including creation, retrieval, update, and deletion operations.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly UserManager<User> userManager;
    private readonly ILogger<ProjectService> logger;

    /// <summary>
/// Initializes a new instance of the <see cref="ProjectService"/> class.
/// </summary>
/// <param name="timeForgeRepository">The repository for data access.</param>
/// <param name="userManager"></param>
/// <param name="logger">The logger instance.</param>
public ProjectService(ITimeForgeRepository timeForgeRepository, 
        ILogger<ProjectService> logger,
        UserManager<User> userManager)
    {
        this.timeForgeRepository = timeForgeRepository ?? throw new ArgumentNullException(nameof(timeForgeRepository));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
/// Creates a new project using the provided input model.
/// </summary>
/// <param name="inputModel">The input model containing project data.</param>
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
                Id = inputModel.Id!,
                Name = inputModel.Name,
                IsPublic = inputModel.IsPublic,
                DueDate = inputModel.DueDate,
                UserId = inputModel.UserId,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await this.timeForgeRepository.AddAsync(newProject);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully created a new project with Name: {Name}.", newProject.Name);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in CreateProjectAsync.");
            throw new InvalidOperationException("An error occurred while creating the project.", ex);
        }
    }

    /// <summary>
/// Retrieves a project by its unique identifier.
/// </summary>
/// <param name="projectId">The project ID.</param>
/// <returns>The project view model.</returns>
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

                .Include(p => p.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (project == null)
            {
                this.logger.LogWarning("Project with Id {ProjectId} not found in GetProjectByIdAsync.", projectId);
                throw new InvalidOperationException($"Project with id {projectId} not found.");
            }



            var viewModel = new ProjectViewModel()
            {
                Id = project.Id,
                Name = project.Name,
                DueDate = project.DueDate?.ToString("dd/MM/yyyy"),
                CreatedBy = project.CreatedBy.UserName ?? string.Empty,
                UserId = project.UserId,

                IsPublic = project.IsPublic
            };

            this.logger.LogInformation("Successfully retrieved project with Id: {ProjectId}.", projectId);
            return viewModel;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex,
                "An error occurred in GetProjectByIdAsync while fetching project with Id: {ProjectId}.", projectId);
            throw;
        }
    }

    /// <summary>
/// Retrieves all projects for a user.
/// </summary>
/// <param name="userId">The user ID.</param>
/// <returns>A collection of project view models.</returns>
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

                .Include(p => p.CreatedBy)
                .Include(p => p.Tasks)
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.DueDate,
                    CreatedBy = p.CreatedBy.UserName,

                    Tasks = p.Tasks.Select(t => new { t.Name, t.IsCompleted }).ToList(),
                    p.IsPublic
                })
                .ToListAsync();

            // Perform projection to ViewModel in memory
            var projectViewModels = projects.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DueDate = p.DueDate?.ToString("dd/MM/yyyy"), // Format handled in memory
                CreatedBy = p.CreatedBy ?? string.Empty,

                Tasks = p.Tasks.Select(task => new TaskViewModel()
                {
                    Name = task.Name,
                    IsCompleted = task.IsCompleted
                }).ToList(),
                IsPublic = p.IsPublic
            }).ToList();

            this.logger.LogInformation(
                "GetAllProjectsAsync successfully retrieved {ProjectCount} projects for userId {UserId}.",
                projectViewModels.Count, userId);
            return projectViewModels;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in GetAllProjectsAsync for userId {UserId}.", userId);
            throw new InvalidOperationException("An error occurred while retrieving projects", ex);
        }
    }

    /// <summary>
    /// Retrieves all projects for a user with paging.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A collection of project view models.</returns>
    public async Task<IEnumerable<ProjectViewModel>> GetAllProjectsAsync(string userId, int pageNumber, int pageSize)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning("GetAllProjectsAsync was called with a null or empty userId.");
                throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty");
            }

            // Fetch only what is needed from the database
            var projects = await this.timeForgeRepository.All<Project>(
                    e => e.UserId == userId || 
                         e.AssignedUserId == userId)

                .Include(p => p.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.DueDate,
                    p.AssignedUserId,
                    CreatedBy = p.CreatedBy.UserName,

                    p.IsPublic,
                    userId = p.UserId
                })
                .ToListAsync();

            // Perform projection to ViewModel in memory
            var projectViewModels = projects.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DueDate = p.DueDate?.ToString("dd/MM/yyyy"), // Format handled in memory
                CreatedBy = p.CreatedBy ?? string.Empty,

                IsPublic = p.IsPublic,
                UserId = p.userId,
                AssignedToUserId = p.AssignedUserId
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



    /// <summary>
/// Deletes a project by its unique identifier.
/// </summary>
/// <param name="projectId">The project ID.</param>
public async Task DeleteProject(string projectId)
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

            this.timeForgeRepository.Delete(project);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully deleted project with Id: {ProjectId}.", projectId);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in DeleteProject for projectId: {ProjectId}.", projectId);
            throw;
        }
    }

    /// <summary>
/// Gets the count of projects for a user.
/// </summary>
/// <param name="userId">The user ID.</param>
/// <returns>The number of projects.</returns>
public async Task<int> GetProjectsCountAsync(string userId)
    {
        return await timeForgeRepository.All<Project>()
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .CountAsync();
    }



    /// <summary>
/// Updates an existing project with new data.
/// </summary>
/// <param name="inputModel">The input model containing updated project data.</param>
public async Task UpdateProject(ProjectInputModel inputModel)
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

            project.Name = inputModel.Name;
            project.DueDate = inputModel.DueDate;
            project.IsPublic = inputModel.IsPublic;
            project.LastModified = DateTime.UtcNow;
            project.UserId = inputModel.UserId ?? project.UserId;


            this.timeForgeRepository.Update(project);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully updated project with Id: {Id}.", inputModel.Id);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "An error occurred in UpdateProject for projectId: {Id}.", inputModel?.Id);
            throw;
        }
    }

    /// <summary>
/// Adds a task to a project.
/// </summary>
/// <param name="projectId">The project ID.</param>
/// <param name="taskId">The task ID.</param>
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

    /// <summary>
/// Removes a task from a project.
/// </summary>
/// <param name="projectId">The project ID.</param>
/// <param name="taskId">The task ID.</param>
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



    public async Task AssignProjectToUser(string projectId, string userId)
    {
        try
        {
            if (String.IsNullOrEmpty(projectId) || String.IsNullOrEmpty(userId))
            {
                this.logger.LogWarning(
                    "AssignProjectToUser was called with invalid parameters. ProjectId: {ProjectId}, UserId: {UserId}.",
                    projectId, userId);
                throw new ArgumentNullException(projectId, userId);
            }

            this.logger.LogInformation(
                "AssignProjectToUser was called with valid parameters. ProjectId: {ProjectId}, UserId: {UserId}.",
                projectId, userId);
            var user = await this.userManager.FindByIdAsync(userId);
            var project = await this.timeForgeRepository.GetByIdAsync<Project>(projectId);

            if (user == null || project == null)
            {
                this.logger.LogWarning(
                    "Either user or project was not found in AssignProjectToUser. ProjectId: {ProjectId}, UserId: {UserId}.",
                    projectId, userId);
                throw new ArgumentException("Both user and project must exist.");
            }

            project.UserId = userId;
            this.timeForgeRepository.Update(project);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully assigned project with Id {ProjectId} to user with Id {UserId}.",
                projectId, userId);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception)
        {
            this.logger.LogError("An error occurred in AssignProjectToUser for projectId: {ProjectId}, userId: {UserId}.", projectId, userId);
            throw;
        }
    }
}