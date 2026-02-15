using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TimeForge.Services.Interfaces;
using TimeForge.Web.Controllers;

namespace TimeForge.Tests;

[TestFixture]
public class TimeEntryControllerTests
{
    private Mock<ITimeEntryService> timeEntryServiceMock;
    private Mock<ILogger<TimeEntryController>> loggerMock;
    private TimeEntryController timeEntryController;
    private const string UserId = "test-user-id";

    [SetUp]
    public void Setup()
    {
        timeEntryServiceMock = new Mock<ITimeEntryService>();
        loggerMock = new Mock<ILogger<TimeEntryController>>();
        timeEntryController = new TimeEntryController(timeEntryServiceMock.Object, loggerMock.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, UserId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        timeEntryController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task Start_ValidTaskId_ReturnsOk()
    {
        // Arrange
        var taskId = "task-1";
        timeEntryServiceMock.Setup(s => s.StartEntryAsync(taskId, UserId)).Returns(Task.CompletedTask);

        // Act
        var result = await timeEntryController.Start(taskId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task Stop_ValidEntryId_ReturnsOk()
    {
        // Arrange
        var entryId = "entry-1";
        timeEntryServiceMock.Setup(s => s.StopEntryAsync(entryId)).Returns(Task.CompletedTask);

        // Act
        var result = await timeEntryController.Stop(entryId);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }
}
