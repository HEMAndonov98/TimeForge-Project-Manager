using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TimeForge.Infrastructure;
using TimeForge.Infrastructure.Repositories;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services;
using TimeForge.ViewModels.Project;

namespace TimeForge.Tests;

[TestFixture]
public class ProjectServiceTests
{
    private Mock<ITimeForgeRepository> timeForgeRepositoryMock;
    private Mock<ILogger<ProjectService>> loggerMock;
    private ProjectService projectService;

    [SetUp]
    public void Setup()
    {
        timeForgeRepositoryMock = new Mock<ITimeForgeRepository>();
        loggerMock = new Mock<ILogger<ProjectService>>();
        projectService = new ProjectService(timeForgeRepositoryMock.Object, loggerMock.Object);
    }

    #region CreateProjectAsync

    [Test]
    public async Task CreateProjectAsync_ValidInput_AddsAndSavesProject()
    {
        // Arrange
        var input = new ProjectInputModel() { Id = "123",
            Name = "Test Project",
            DueDate  = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
            IsPublic = true,
            UserId = "user1"
        };
        
        // Setup
        timeForgeRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Project>()))
            .ReturnsAsync(new Project { Id = input.Id });
        
        // Act
        await this.projectService.CreateProjectAsync(input);
        
        // Assert
        this.timeForgeRepositoryMock.Verify(r => r.AddAsync(It.Is<Project>(
            p => p.Name == input.Name &&
                 p.DueDate == input.DueDate &&
                 p.IsPublic == input.IsPublic &&
                 p.UserId == input.UserId)), Times.Once);
        this.timeForgeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    
    [Test]
    public void CreateProjectAsync_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        ProjectInputModel input = null;
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.CreateProjectAsync(input));
    }

    #endregion
    
    #region GetProjectByIdAsync

    [Test]
    public async Task GetProjectByIdAsync_ValidId_ReturnsProjectViewModel()
    {
        // Arrange
        await using var inMemoryDbContext = InitialiseInMemoryDbContext(); 
        var inMemoryRepository = new TimeForgeRepository(inMemoryDbContext);
        var inMemoryProjectService = new ProjectService(inMemoryRepository, loggerMock.Object);
        
        var testProject = await AddTestProjectToInMemoryRepository(inMemoryRepository);
            

        // Act
        var result = await inMemoryProjectService.GetProjectByIdAsync(testProject.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(testProject.Id));
        Assert.That(result.Name, Is.EqualTo(testProject.Name));
        Assert.That(result, Is.TypeOf<ProjectViewModel>());
    }

    [Test]
    public void GetProjectByIdAsync_NullOrEmptyId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.GetProjectByIdAsync(null));
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.GetProjectByIdAsync(String.Empty));
    }

    [Test]
    public async Task GetProjectByIdAsync_NotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var inMemoryDbContext = InitialiseInMemoryDbContext(); 
        var inMemoryRepository = new TimeForgeRepository(inMemoryDbContext);
        var inMemoryProjectService = new ProjectService(inMemoryRepository, loggerMock.Object);
        
        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => inMemoryProjectService.GetProjectByIdAsync("missing"));
    }
    
    #endregion

    #region GetAllProjectsAsync

    [Test]
    public void GetAllProjectsAsync_NullOrEmptyUserId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.GetAllProjectsAsync(null));
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.GetAllProjectsAsync(String.Empty));
    }

    [Test]
    public async Task GetAllProjectsAsync_ValidUserId_ReturnsProjectViewModelList()
    {
        // Arrange
        await using var inMemoryDbContext = InitialiseInMemoryDbContext(); 
        var inMemoryRepository = new TimeForgeRepository(inMemoryDbContext);
        var inMemoryProjectService = new ProjectService(inMemoryRepository, loggerMock.Object);
        
        var testProject = await AddTestProjectToInMemoryRepository(inMemoryRepository);
        
        // Act
        var result = await inMemoryProjectService.GetAllProjectsAsync(testProject.UserId);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));

        var projectViewModel = result.First();

        // Verify all fields match expected values
        Assert.That(projectViewModel.Id, Is.EqualTo(testProject.Id));
        Assert.That(projectViewModel.Name, Is.EqualTo(testProject.Name));
        Assert.That(projectViewModel.CreatedBy, Is.EqualTo(testProject.CreatedBy.UserName));
        Assert.That(projectViewModel.IsPublic, Is.EqualTo(testProject.IsPublic));
        Assert.That(projectViewModel.Tasks.Count, Is.EqualTo(testProject.Tasks.Count));
        Assert.That(projectViewModel.Tags.Count, Is.EqualTo(testProject.ProjectTags.Count));
        
        // // Verify Tasks collection
        Assert.That(projectViewModel.Tasks, Is.Not.Null);
        Assert.That(projectViewModel.Tasks.Count, Is.EqualTo(testProject.Tasks.Count));
        var task = projectViewModel.Tasks.First();
        Assert.That(task.Name, Is.EqualTo(testProject.Tasks.First().Name));
        Assert.That(task.IsCompleted, Is.EqualTo(testProject.Tasks.First().IsCompleted));;
        
        // Verify Tags collection
        Assert.That(projectViewModel.Tags, Is.Not.Null);
        Assert.That(projectViewModel.Tags.Count, Is.EqualTo(testProject.ProjectTags.Count));
        var tag = projectViewModel.Tags.First();
        Assert.That(tag.Id, Is.EqualTo(testProject.ProjectTags.First().Tag.Id));
        Assert.That(tag.Name, Is.EqualTo(testProject.ProjectTags.First().Tag.Name));
        
        // Verify all items are correct type
        CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ProjectViewModel));
    }

    #endregion

    #region DeleteProject

    [Test]
    public async Task DeleteProject_ValidId_RunsDelete()
    {
        var input = new Project() { Id = "123", Name = "Test Project" };
        this.timeForgeRepositoryMock.Setup(r => r.GetByIdAsync<Project>("123"))
            .ReturnsAsync(input);
        
        await this.projectService.DeleteProject("123");
        
        this.timeForgeRepositoryMock.Verify(r => r.Delete(It.IsAny<Project>()), Times.Once);
        this.timeForgeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteProject_NullOrEmptyId_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.DeleteProject(null));
        Assert.ThrowsAsync<ArgumentNullException>(() => this.projectService.DeleteProject(String.Empty));
    }

    [Test]
    public void DeleteProject_NotFound_ThrowArgumentException()
    {
        Project input = null;
        this.timeForgeRepositoryMock.Setup(r => r.GetByIdAsync<Project>("123"))
            .ReturnsAsync(input);
        
        
        Assert.ThrowsAsync<ArgumentException>(() => this.projectService.DeleteProject("123"));
    }

    #endregion

    #region GetProjectsCountAsync

    [Test]
    public async Task GetProjectsCountAsync_Returns_ProjectsCount_UsingInMemoryDb()
    {
        // Arrange
        var userId = "testUserId";
        var expectedCount = 5;

        // Initialize the in-memory database
        await using var inMemoryDbContext = InitialiseInMemoryDbContext();
        var inMemoryRepository = new TimeForgeRepository(inMemoryDbContext);
        var inMemoryProjectService = new ProjectService(inMemoryRepository, loggerMock.Object);

        // Add test data to the in-memory database
        for (int i = 0; i < expectedCount; i++)
        {
            await inMemoryRepository.AddAsync(new Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Project {i + 1}",
                UserId = userId
            });
        }
        await inMemoryRepository.SaveChangesAsync();

        // Act
        var result = await inMemoryProjectService.GetProjectsCountAsync(userId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCount));
    }

    #endregion

    #region UpdateProject

    

    #endregion
    
    #region HelperMethods

    private TimeForgeDbContext InitialiseInMemoryDbContext()
    {
        var dbContextOptions = new DbContextOptionsBuilder<TimeForgeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Use a unique database name
            .Options;
        
        var inMemoryDbContext = new TimeForgeDbContext(dbContextOptions);
        return inMemoryDbContext;
    }

    private async Task<Project> AddTestProjectToInMemoryRepository(TimeForgeRepository inMemoryRepository)
    {
        var testTask = new ProjectTask() {Id = "123", Name = "Test Task", ProjectId = "123"};
        var projectTaskList = new List<ProjectTask>();
        projectTaskList.Add(testTask);
        
        var testTag = new Tag() { Id = "123", Name = "Test Tag", UserId = "user1" };
        var projectTagList = new List<ProjectTag>();
        projectTagList.Add(new ProjectTag() { ProjectId = "123", Tag = testTag });
            
        var testUser = new User() { Id = "user1", UserName = "Test User" };
            
        var testProject = new Project()
        {
            Id = "123",
            Name = "Test Project",
            UserId = "user1",
            ProjectTags = projectTagList,
            CreatedBy = testUser,
            Tasks = projectTaskList,
        };

        await inMemoryRepository.AddAsync(testProject);
        await inMemoryRepository.SaveChangesAsync();

        return testProject;
    }
    #endregion
}