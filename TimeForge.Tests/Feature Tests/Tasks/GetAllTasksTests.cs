using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Tasks.GetAll;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Database;

namespace TimeForge.Tests.Feature_Tests.Tasks;

public class GetAllTasksTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task GetAll_Tasks_Successfully()
    {
        // 1. Authenticate
        var (token, userId) = await AuthenticateUserAsync("getall_tasks_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Project & Tasks
        string projectId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var project = Models.Project.Create(userId, "Get All Project", "Desc", null);
            project.AddTask("Task 1");
            project.AddTask("Task 2");
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            projectId = project.Id;
        }

        // 3. Get All Tasks via Endpoint
        var (response, result) = await app.Client.GETAsync<GetAllTasksEndpoint, GetAllTasksRequest, GetAllTasksResponse>(new() { ProjectId = projectId });

        // 4. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Tasks.Count.ShouldBe(2);
        result.Tasks.ShouldContain(t => t.Name == "Task 1");
        result.Tasks.ShouldContain(t => t.Name == "Task 2");
    }

    [Fact]
    public async Task GetAll_Tasks_NotFound_If_Project_DoesNotExist()
    {
        // 1. Authenticate
        var (token, _) = await AuthenticateUserAsync("getall_tasks_404@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Try get tasks for non-existent project
        var (response, _) = await app.Client.GETAsync<GetAllTasksEndpoint, GetAllTasksRequest, GetAllTasksResponse>(new() { ProjectId = "non-existent-id" });

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_Tasks_NotFound_If_Not_Owner()
    {
        // 1. User A creates a project
        var (tokenA, userIdA) = await AuthenticateUserAsync("userA_getall@example.com");
        string projectId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var project = Models.Project.Create(userIdA, "User A Project", "Desc", null);
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            projectId = project.Id;
        }

        // 2. User B tries to get User A's project tasks
        var (tokenB, _) = await AuthenticateUserAsync("userB_getall@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        var (response, _) = await app.Client.GETAsync<GetAllTasksEndpoint, GetAllTasksRequest, GetAllTasksResponse>(new() { ProjectId = projectId });

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
