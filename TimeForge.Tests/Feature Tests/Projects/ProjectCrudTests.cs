using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Projects.Create;
using TimeForge.Api.Features.Projects.GetAll;
using TimeForge.Api.Features.Projects.Update;
using TimeForge.Api.Features.Projects.Delete;
using TimeForge.Api.Features.Projects.GetById;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Projects;

public class ProjectCrudTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
{
    private async Task<(string Token, string UserId)> AuthenticateUserAsync(string email)
    {
        var password = "SecurePassword123!";
        await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(new()
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = password
        });

        var (_, loginResult) = await app.Client.POSTAsync<LoginEndpoint, LoginRequest, LoginResponse>(new()
        {
            Email = email,
            Password = password
        });

        return (loginResult.Token, loginResult.UserId);
    }

    [Fact]
    public async Task Project_Full_Lifecycle_Test()
    {
        // 1. Authenticate
        var (token, _) = await AuthenticateUserAsync("project_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Project
        var createRequest = new CreateProjectRequest
        {
            Name = "Lifecycle Project",
            Description = "Testing full lifecycle",
            Color = "green",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var (createResponse, createResult) = await app.Client.POSTAsync<CreateProjectEndpoint, CreateProjectRequest, CreateProjectResponse>(createRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createResult.Name.ShouldBe(createRequest.Name);
        var projectId = createResult.Id;

        // 3. Get All Projects
        var (getAllResponse, getAllResult) = await app.Client.GETAsync<GetProjectsEndpoint, GetProjectsResponse>();
        getAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        getAllResult.Projects.ShouldContain(p => p.Id == projectId);

        // 4. Get By ID
        var (getByIdResponse, getByIdResult) = await app.Client.GETAsync<GetProjectByIdEndpoint, GetProjectByIdRequest, GetProjectByIdResponse>(new GetProjectByIdRequest()
            { Id = projectId });
        getByIdResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        getByIdResult.Id.ShouldBe(projectId);
        getByIdResult.Name.ShouldBe(createRequest.Name);

        // 5. Update Project
        var updateRequest = new UpdateProjectRequest
        {
            Id = projectId,
            Name = "Updated Project Name",
            Description = "Updated Description",
            Color = "red"
        };
        var (updateResponse, updateResult) = await app.Client.PUTAsync<UpdateProjectEndpoint, UpdateProjectRequest, UpdateProjectResponse>(updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        updateResult.Name.ShouldBe(updateRequest.Name);
        updateResult.Color.ShouldBe(updateRequest.Color);

        // 6. Delete Project
        var deleteResponse = await app.Client.DELETEAsync<DeleteProjectEndpoint, DeleteProjectRequest>(new DeleteProjectRequest { Id = projectId });
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 7. Verify DELETED (404)
        var (getDeletedResponse, _) = await app.Client.GETAsync<GetProjectByIdEndpoint, GetProjectByIdRequest, GetProjectByIdResponse>(new GetProjectByIdRequest { Id = projectId });
        getDeletedResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Project_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var (token, _) = await AuthenticateUserAsync("invalid_project@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateProjectRequest { Name = "" }; // Invalid: Name is required

        // Act
        var (response, _) = await app.Client.POSTAsync<CreateProjectEndpoint, CreateProjectRequest, CreateProjectResponse>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Access_Other_User_Project_ReturnsNotFound()
    {
        // 1. User A creates a project
        var (tokenA, _) = await AuthenticateUserAsync("userA@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
        
        var (_, projectA) = await app.Client.POSTAsync<CreateProjectEndpoint, CreateProjectRequest, CreateProjectResponse>(new() { Name = "User A Project" });

        // 2. User B tries to access User A's project
        var (tokenB, _) = await AuthenticateUserAsync("userB@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        var (response, _) = await app.Client.GETAsync<GetProjectByIdEndpoint, GetProjectByIdRequest, GetProjectByIdResponse>(new GetProjectByIdRequest { Id = projectA.Id });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
