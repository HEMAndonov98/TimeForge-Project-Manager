using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.List;
using TimeForge.Models;

namespace TimeForge.Services;
//TODO Add xml documentation when finished implementing
public class TaskCollectionService : ITaskCollectionService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly ILogger<TaskCollectionService> logger;
    private readonly UserManager<User> userManager;
    
    
    public Task CreateTaskCollectionAsync(TaskCollectionInputModel inputModel)
    {
        throw new NotImplementedException();
    }

    public Task CreateTaskItemAsync(TaskItemInputModel inputModel)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTaskCollection(string taskCollectionId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTaskItem(string taskItemId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskCollectionViewModel>> GetAllTaskCollectionsAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskItemViewModel>> GetAllTaskItemsForTaskCollectionAsync(string taskCollectionId)
    {
        throw new NotImplementedException();
    }
}