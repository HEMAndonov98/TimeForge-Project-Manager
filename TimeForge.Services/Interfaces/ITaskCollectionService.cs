using TimeForge.ViewModels.List;

namespace TimeForge.Services.Interfaces;

/// <summary>
/// Provides methods for managing task collections and individual tasks.
/// </summary>
public interface ITaskCollectionService
{
    /// <summary>
    /// Creates a new task collection with the specified details.
    /// </summary>
    /// <param name="inputModel">The input model containing the details of the task collection to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateTaskCollectionAsync(TaskCollectionInputModel inputModel);

    /// <summary>
    /// Creates a new task item within a task collection.
    /// </summary>
    /// <param name="inputModel">The input model containing the details of the task item to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateTaskItemAsync(TaskItemInputModel inputModel);

    /// <summary>
    /// Deletes a task collection by its ID.
    /// </summary>
    /// <param name="taskCollectionId">The ID of the task collection to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteTaskCollection(string taskCollectionId);

    /// <summary>
    /// Deletes a task item by its ID.
    /// </summary>
    /// <param name="taskItemId">The ID of the task item to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteTaskItem(string taskItemId);

    /// <summary>
    /// Retrieves all task collections associated with a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose task collections are to be retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of task collection view models.
    /// </returns>
    Task<IEnumerable<TaskCollectionViewModel>> GetAllTaskCollectionsAsync(string userId);

    /// <summary>
    /// Retrieves all task items for a specific task collection.
    /// </summary>
    /// <param name="taskCollectionId">The ID of the task collection whose task items are to be retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of task item view models.
    /// </returns>
    Task<IEnumerable<TaskItemViewModel>> GetAllTaskItemsForTaskCollectionAsync(string taskCollectionId);
}