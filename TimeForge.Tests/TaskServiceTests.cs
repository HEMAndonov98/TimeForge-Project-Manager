using Microsoft.Extensions.Logging;
using Moq;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services;
using TimeForge.ViewModels.Task;

namespace TimeForge.Tests;

[TestFixture]
public class TaskServiceTests
{
    private Mock<ITimeForgeRepository> repoMock;
    private Mock<ILogger<TaskService>> loggerMock;
    private TaskService taskService;
    
    [SetUp]
    public void Setup()
    {
        // Arrange shared mocks and system under test
        repoMock = new Mock<ITimeForgeRepository>();
        loggerMock = new Mock<ILogger<TaskService>>();
        taskService = new TaskService(repoMock.Object, loggerMock.Object);
    }
    
    #region CreateTaskAsync
    
    [Test]
    public async Task CreateTaskAsync_WithValidInput_AddsTaskAndSaves()
    {
        // Arrange
        var repoMock = new Mock<ITimeForgeRepository>();
        var loggerMock = new Mock<ILogger<TaskService>>();
        var service = new TaskService(repoMock.Object, loggerMock.Object);

        var inputModel = new TaskInputModel
        {
            Name = "Test Task",
            ProjectId = "project123"
        };

        // Act
        await service.CreateTaskAsync(inputModel);

        // Assert
        repoMock.Verify(r => r.AddAsync(It.Is<ProjectTask>(t => t.Name == "Test Task" && t.ProjectId == "project123")), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateTaskAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        TaskInputModel inputModel = null;
        var service = new TaskService(new Mock<ITimeForgeRepository>().Object, new Mock<ILogger<TaskService>>().Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateTaskAsync(inputModel));
    }
    
    #endregion

    #region CompleteTask
    
    [Test]
    public async Task CompleteTask_WithValidId_SetsCompletionAndSaves()
    {
        var repoMock = new Mock<ITimeForgeRepository>();
        var loggerMock = new Mock<ILogger<TaskService>>();
        var task = new ProjectTask { Id = "task123", IsCompleted = false };

        repoMock.Setup(r => r.GetByIdAsync<ProjectTask>("task123")).ReturnsAsync(task);
    
        var service = new TaskService(repoMock.Object, loggerMock.Object);

        await service.CompleteTask("task123");

        Assert.IsTrue(task.IsCompleted);
        Assert.NotNull(task.CompletionDate);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CompleteTask_TaskNotFound_ThrowsInvalidOperationException()
    {
        var repoMock = new Mock<ITimeForgeRepository>();
        repoMock.Setup(r => r.GetByIdAsync<ProjectTask>("missing")).ReturnsAsync((ProjectTask)null);

        var service = new TaskService(repoMock.Object, new Mock<ILogger<TaskService>>().Object);

        Assert.ThrowsAsync<InvalidOperationException>(() => service.CompleteTask("missing"));
    }
    

    #endregion

    #region DeleteTaskAsync

    [Test]
    public async Task DeleteTaskAsync_TaskExists_DeletesAndSaves()
    {
        var repoMock = new Mock<ITimeForgeRepository>();
        var task = new ProjectTask { Id = "taskToDelete" };
        repoMock.Setup(r => r.GetByIdAsync<ProjectTask>("taskToDelete")).ReturnsAsync(task);

        var service = new TaskService(repoMock.Object, new Mock<ILogger<TaskService>>().Object);

        await service.DeleteTaskAsync("taskToDelete");

        repoMock.Verify(r => r.Delete(task), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }


    #endregion

    #region GetTaskByIdAsync

    //TODO create an in memmory db to test these methods

    #endregion

    #region GetTasksByProjectIdAsync

    //TODO create an in memmory db to test these methods

    #endregion

}