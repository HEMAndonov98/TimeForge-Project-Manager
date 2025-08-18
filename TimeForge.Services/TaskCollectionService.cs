using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.List;
using TimeForge.Models;

namespace TimeForge.Services;
//TODO Add xml documentation when finished implementing
public class TaskCollectionService(
    ITimeForgeRepository timeForgeRepository,
    ILogger<TaskCollectionService> logger,
    UserManager<User> userManager)
    : ITaskCollectionService
{
    public async Task CreateTaskCollectionAsync(TaskCollectionInputModel inputModel)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(inputModel.UserId))
            {
                logger.LogError($"User ID is empty or whitespace in: {nameof(TaskCollectionService)}/{nameof(CreateTaskCollectionAsync)}");
                throw new ArgumentNullException(nameof(inputModel.UserId));
            }

            if (await userManager.FindByIdAsync(inputModel.UserId) == null)
            {
                logger.LogError($"User with ID: {inputModel.UserId} does not exist in: {nameof(TaskCollectionService)}/{nameof(CreateTaskCollectionAsync)}");
                throw new InvalidOperationException($"User with ID: {inputModel.UserId} does not exist");
            }

            TaskCollection newTaskCollection = new TaskCollection()
            {
                ListName = inputModel.Title,
                UserId = inputModel.UserId,
            };
            
            await timeForgeRepository.AddAsync(newTaskCollection);
            await timeForgeRepository.SaveChangesAsync();
            logger.LogInformation("Successfully created a new task collection for user with ID: {UserId}", inputModel.UserId);
            
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogError($"Error occurred while creating a new task collection for user with ID: {inputModel.UserId} at: {nameof(TaskCollectionService)}/{nameof(CreateTaskCollectionAsync)}");
            throw;
        }
    }

    public async Task CreateTaskItemAsync(TaskItemInputModel inputModel)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(inputModel.TaskCollectionId))
            {
                logger.LogError(
                    $"TaskCollection ID is empty or whitespace in: {nameof(TaskCollectionService)}/{nameof(CreateTaskItemAsync)}");
                throw new ArgumentNullException(nameof(inputModel.TaskCollectionId));
            }

            var taskCollectionExists = await timeForgeRepository
                .All<TaskCollection>()
                .AnyAsync(tc => tc.Id == inputModel.TaskCollectionId);

            if (!taskCollectionExists)
            {
                logger.LogError(
                    $"TaskCollection with ID: {inputModel.TaskCollectionId} does not exist in: {nameof(TaskCollectionService)}/{nameof(CreateTaskItemAsync)}");
                throw new InvalidOperationException(
                    $"TaskCollection with ID: {inputModel.TaskCollectionId} does not exist");
            }

            TaskItem newTaskItem = new TaskItem()
            {
                Title = inputModel.Title,
                TaskCollectionId = inputModel.TaskCollectionId,
                IsCompleted = false,
            };

            await timeForgeRepository.AddAsync(newTaskItem);
            await timeForgeRepository.SaveChangesAsync();
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task DeleteTaskCollection(string taskCollectionId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTaskItem(string taskItemId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<TaskCollectionViewModel>> GetAllTaskCollectionsAsync(string userId)
    {

        try
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                logger.LogError($"User ID is empty or whitespace in: {nameof(TaskCollectionService)}/{nameof(GetAllTaskCollectionsAsync)}");
                throw new ArgumentNullException(nameof(userId));
            }

            if (await userManager.FindByIdAsync(userId) == null)
            {
                logger.LogError($"User with ID: {userId} does not exist in: {nameof(TaskCollectionService)}/{nameof(GetAllTaskCollectionsAsync)}");
                throw new InvalidOperationException($"User with ID: {userId} does not exist");       
            }
        
            var taskCollections = await timeForgeRepository
                .All<TaskCollection>(tc => tc.UserId == userId)
                .Include(tc => tc.TaskItems)
                .AsNoTracking()
                .Select(tc => new TaskCollectionViewModel()
                {
                    Id = tc.Id,
                    Title = tc.ListName,
                    Tasks = tc.TaskItems
                        .Select(ti => new TaskItemViewModel()
                        {
                            Id = ti.Id,
                            Description = ti.Title,
                            IsCompleted = ti.IsCompleted
                        })
                        .ToList()
                })
                .ToListAsync();
        
            return taskCollections;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogError($"Error occurred while retrieving task collections for user with ID: {userId} at: {nameof(TaskCollectionService)}/{nameof(GetAllTaskCollectionsAsync)}");
            throw;
        }
    }

    public Task<IEnumerable<TaskItemViewModel>> GetAllTaskItemsForTaskCollectionAsync(string taskCollectionId)
    {
        throw new NotImplementedException();
    }

    public async Task<TaskCollectionViewModel> GetTaskCollectionByIdAsync(string taskCollectionId)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(taskCollectionId))
            {
                logger.LogError($"TaskCollection ID is empty or whitespace in: {nameof(TaskCollectionService)}/{nameof(GetTaskCollectionByIdAsync)}");
                throw new ArgumentNullException(nameof(taskCollectionId));
            }

            TaskCollectionViewModel? taskCollectionViewModel = await timeForgeRepository
                .All<TaskCollection>(tc => tc.Id == taskCollectionId)
                .Include(tc => tc.TaskItems)
                .AsNoTracking()
                .Select(tc => new TaskCollectionViewModel()
                {
                    Id = tc.Id,
                    Title = tc.ListName,
                    Tasks = tc.TaskItems
                        .Select(ti => new TaskItemViewModel()
                        {
                            Id = ti.Id,
                            Description = ti.Title,
                            IsCompleted = ti.IsCompleted
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (taskCollectionViewModel == null)
            {
                logger.LogWarning($"TaskCollection with ID: {taskCollectionId} not found in: {nameof(TaskCollectionService)}/{nameof(GetTaskCollectionByIdAsync)}");
                throw new InvalidOperationException($"TaskCollection with ID: {taskCollectionId} not found");
            }
        
            logger.LogInformation("Successfully retrieved task collection with ID: {TaskCollectionId}", taskCollectionId);
            return taskCollectionViewModel;
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogError($"Error occurred while retrieving task collection with ID: {taskCollectionId} at: {nameof(TaskCollectionService)}/{nameof(GetTaskCollectionByIdAsync)}");
            throw;
        }
    }
}