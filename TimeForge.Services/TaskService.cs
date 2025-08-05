using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;
using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.Services;

public class TaskService : ITaskService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly ILogger<TaskService> logger;

    public TaskService(ITimeForgeRepository timeForgeRepository, ILogger<TaskService> logger)
    {
        this.timeForgeRepository = timeForgeRepository;
        this.logger = logger;
        
        this.logger.LogInformation("Initializing TaskService with timeForgeRepository");
    }
    
    public async Task CreateTaskAsync(TaskInputModel inputModel)
    {
        try
        {
            if (inputModel == null)
            {
                this.logger.LogError("Input model is null");
                throw new ArgumentNullException();
            }

            this.logger.LogInformation("Creating task with input model {InputModel}", inputModel);

            var newTask = new ProjectTask()
            {
                ProjectId = inputModel.ProjectId,
                Name = inputModel.Name,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await this.timeForgeRepository.AddAsync(newTask);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully created task with ID: {TaskId}", newTask.Id);
        }
        catch (ArgumentNullException)
        {
            this.logger.LogError("Input model is null");
            throw new ArgumentNullException();
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error occurred while creating task: {TaskName}", inputModel?.Name ?? "unknown");
            throw new InvalidOperationException("Error occurred while creating task", e);
        }
    }

    public async Task CompleteTask(string taskId)
    {
        this.logger.LogInformation("Completing task with ID: {TaskId}", taskId);
        try
        {
            var task = await this.timeForgeRepository.GetByIdAsync<ProjectTask>(taskId);
            if (task == null)
            {
                this.logger.LogError("Task with ID: {TaskId} does not exist", taskId);
                throw new InvalidOperationException("Task with ID does not exist");
            }

            var completionTime = DateTime.UtcNow;

            task.IsCompleted = true;
            task.CompletionDate = completionTime;
            task.LastModified = completionTime;
            
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully completed task with ID: {TaskId}", taskId);
        }
        catch (Exception e)
        {
           this.logger.LogError(e, "Error occurred while completing task with ID: {TaskId}", taskId);
            throw new InvalidOperationException("Error occurred while completing task", e);
        }
    }

    public async Task<TaskViewModel> GetTaskByIdAsync(string taskId)
    {
        this.logger.LogInformation("Retrieving task with ID: {TaskId}", taskId);
        try
        {
            var task = await this.timeForgeRepository.All<ProjectTask>(t => t.Id == taskId)
                .Include(t => t.TimeEntries)
                .ThenInclude(te => te.CreatedBy.UserName)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (task == null)
            {
                this.logger.LogError("Task with ID: {TaskId} does not exist", taskId);
                throw new InvalidOperationException("Task with ID does not exist");
                
            }

            TaskViewModel viewModel = new TaskViewModel()
            {
                Id = task.Id,
                Name = task.Name,
                IsCompleted = task.IsCompleted,
                TimeEntries = task.TimeEntries
                    .Select(te => new TimeEntryViewModel()
                    {
                        Start = te.Start,
                        End = te.End,
                        TaskId = taskId,
                        CreatedBy = te.CreatedBy.UserName ?? string.Empty,
                        Duration = te.Duration ?? TimeSpan.Zero,
                        State = te.State
                    })
                    .ToList()
            };
            this.logger.LogInformation("Successfully retrieved task with ID: {TaskId}", taskId);
            return viewModel;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<IEnumerable<TaskViewModel>> GetTasksByProjectIdAsync(string projectId)
    {
        this.logger.LogInformation("Retrieving tasks for project with ID: {ProjectId}", projectId);
        try
        {
            bool projectExists = await this.timeForgeRepository.All<Project>(p => p.Id == projectId)
                .AsNoTracking()
                .AnyAsync();

            if (!projectExists)
            {
                this.logger.LogError("Project with ID: {ProjectId} does not exist", projectId);
                throw new InvalidOperationException("Project with ID does not exist");
            }

            var tasks = await this.timeForgeRepository.All<ProjectTask>(t => t.ProjectId == projectId)
                .Include(t => t.TimeEntries)
                .ThenInclude(te => te.CreatedBy)
                .AsNoTracking()
                .Select(t => new TaskViewModel()
                {
                    Id = t.Id,
                    IsCompleted = t.IsCompleted,
                    Name = t.Name,
                    TimeEntries = t.TimeEntries
                        .Select(te => new TimeEntryViewModel()
                        {
                            Start = te.Start,
                            End = te.End,
                            TaskId = te.TaskId,
                            CreatedBy = te.CreatedBy.UserName ?? string.Empty,
                            Duration = te.Duration ?? TimeSpan.Zero,
                            State = te.State
                        })
                        .ToList()
                })
                .ToListAsync();

            this.logger.LogInformation("Successfully retrieved tasks for project with ID: {ProjectId}", projectId);
            return tasks;
        }
        catch (Exception e)
        {
           this.logger.LogError(e, "Error occurred while retrieving tasks for project with ID: {ProjectId}", projectId);
            throw new InvalidOperationException("Error occurred while retrieving tasks for project", e);
        }
    }

    public async Task DeleteTaskAsync(string taskId)
    {
        this.logger.LogInformation("Deleting task with ID: {TaskId}", taskId);
        try
        {
            var task = await this.timeForgeRepository.GetByIdAsync<ProjectTask>(taskId);
            if (task == null)
            {
                this.logger.LogError("Task with ID: {TaskId} does not exist", taskId);
                throw new InvalidOperationException("Task with ID does not exist");
            }

            this.timeForgeRepository.Delete(task);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully deleted task with ID: {TaskId}", taskId);
        }
        catch (Exception e)
        {
           this.logger.LogError(e, "Error occurred while deleting task with ID: {TaskId}", taskId);
           throw new InvalidOperationException("Error occurred while deleting task", e);
        }
    }
}