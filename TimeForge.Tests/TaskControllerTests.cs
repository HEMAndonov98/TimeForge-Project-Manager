using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Task;
using TimeForge.ViewModels.TimeEntry;
using TimeForge.Web.Controllers;

namespace TimeForge.Tests;

[TestFixture]
public class TaskControllerTests
{
    private Mock<ITaskService> taskServiceMock;
    private Mock<ITimeEntryService> timeEntryServiceMock;
    private Mock<ILogger<TaskController>> loggerMock;
    private TaskController taskController;

    [SetUp]
    public void Setup()
    {
        taskServiceMock = new Mock<ITaskService>();
        timeEntryServiceMock = new Mock<ITimeEntryService>();
        loggerMock = new Mock<ILogger<TaskController>>();
        taskController = new TaskController(taskServiceMock.Object, timeEntryServiceMock.Object, loggerMock.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        taskController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task Create_ValidInput_ReturnsOk()
    {
        // Arrange
        var input = new TaskInputModel { ProjectId = "project-1", Name = "Test Task" };
        taskServiceMock.Setup(s => s.CreateTaskAsync(input)).Returns(Task.CompletedTask);

        // Act
        var result = await taskController.Create(input);

        // Assert
        Assert.IsInstanceOf<OkResult>(result.Result);
        taskServiceMock.Verify(s => s.CreateTaskAsync(input), Times.Once);
    }

    [Test]
    public async Task Complete_ValidId_StopsTimerAndCompletesTask()
    {
        // Arrange
        var taskId = "task-1";
        var entryId = "entry-1";
        timeEntryServiceMock.Setup(s => s.GetCurrentRunningTimeEntryByUserIdAsync("test-user-id"))
            .ReturnsAsync(new TimeEntryViewModel { Id = entryId, TaskId = taskId });
        timeEntryServiceMock.Setup(s => s.StopEntryAsync(entryId)).Returns(Task.CompletedTask);
        taskServiceMock.Setup(s => s.CompleteTask(taskId)).Returns(Task.CompletedTask);

        // Act
        var result = await taskController.Complete(taskId);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        timeEntryServiceMock.Verify(s => s.StopEntryAsync(entryId), Times.Once);
        taskServiceMock.Verify(s => s.CompleteTask(taskId), Times.Once);
    }

    [Test]
    public async Task GetByProject_ValidId_ReturnsTasks()
    {
        // Arrange
        var projectId = "project-1";
        var tasks = new List<TaskViewModel> { new TaskViewModel { Id = "task-1", Name = "Task 1" } };
        taskServiceMock.Setup(s => s.GetTasksByProjectIdAsync(projectId)).ReturnsAsync(tasks);

        // Act
        var result = await taskController.GetByProject(projectId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(tasks, okResult.Value);
    }

    [Test]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        // Arrange
        var taskId = "task-1";
        taskServiceMock.Setup(s => s.DeleteTaskAsync(taskId)).Returns(Task.CompletedTask);

        // Act
        var result = await taskController.Delete(taskId);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        taskServiceMock.Verify(s => s.DeleteTaskAsync(taskId), Times.Once);
    }
}
