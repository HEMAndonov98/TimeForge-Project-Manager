using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.Project;
using TimeForge.Web.Controllers;

namespace TimeForge.Tests;

[TestFixture]
public class ProjectControllerTests
{
    private Mock<IProjectService> projectServiceMock;
    private Mock<ITaskService> taskServiceMock;
    private Mock<ILogger<ProjectController>> loggerMock;
    private ProjectController projectController;
    private const string UserId = "test-user-id";

    [SetUp]
    public void Setup()
    {
        projectServiceMock = new Mock<IProjectService>();
        taskServiceMock = new Mock<ITaskService>();
        loggerMock = new Mock<ILogger<ProjectController>>();
        projectController = new ProjectController(projectServiceMock.Object, taskServiceMock.Object, loggerMock.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, UserId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        projectController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task Index_ReturnsPagedProjects()
    {
        // Arrange
        var pagedProject = new PagedProjectViewModel { CurrentPage = 1, TotalPages = 1, Projects = new List<ProjectViewModel>() };
        projectServiceMock.Setup(s => s.GetProjectsCountAsync(UserId)).ReturnsAsync(0);
        projectServiceMock.Setup(s => s.GetAllProjectsAsync(UserId, 1, 4)).ReturnsAsync(new List<ProjectViewModel>());

        // Act
        var result = await projectController.Index(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var viewModel = okResult.Value as PagedProjectViewModel;
        Assert.IsNotNull(viewModel);
    }

    [Test]
    public async Task Details_ValidId_ReturnsProject()
    {
        // Arrange
        var projectId = "project-1";
        var project = new ProjectViewModel { Id = projectId, Name = "Test Project" };
        projectServiceMock.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
        taskServiceMock.Setup(s => s.GetTasksByProjectIdAsync(projectId)).ReturnsAsync(new List<TimeForge.ViewModels.Task.TaskViewModel>());

        // Act
        var result = await projectController.Details(projectId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(project, okResult.Value);
    }

    [Test]
    public async Task Create_ValidInput_ReturnsOk()
    {
        // Arrange
        var input = new ProjectInputModel { Name = "New Project" };
        projectServiceMock.Setup(s => s.CreateProjectAsync(input)).Returns(Task.CompletedTask);

        // Act
        var result = await projectController.Create(input);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        // Arrange
        var projectId = "project-1";
        projectServiceMock.Setup(s => s.DeleteProject(projectId)).Returns(Task.CompletedTask);

        // Act
        var result = await projectController.Delete(projectId);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }
}
