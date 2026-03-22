using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Tasks.Delete;
using TimeForge.Database;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Models;

namespace TimeForge.Tests.Feature_Tests.Tasks;

public class DeleteTaskTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task Delete_Task_Successfully()
    {
        // 1. Authenticate
        var (token, userId) = await AuthenticateUserAsync("delete_task_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Project & Task
        string taskId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var project = Models.Project.Create(userId, "Delete Task Project", "Desc", null);
            var task = project.AddTask("Task to Delete");
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            taskId = task.Id;
        }

        // 3. Delete Task via Endpoint
        var response = await app.Client.DELETEAsync<DeleteTaskEndpoint, DeleteTaskRequest>(new() { TaskId = taskId });

        // 4. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 5. Verify in DB (soft delete)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var task = await db.Set<ProjectTask>().IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == taskId);
            task.ShouldNotBeNull();
            task.IsDeleted.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Delete_Task_NotFound_If_Task_DoesNotExist()
    {
        // 1. Authenticate
        var (token, _) = await AuthenticateUserAsync("delete_task_404@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Try delete non-existent task
        var response = await app.Client.DELETEAsync<DeleteTaskEndpoint, DeleteTaskRequest>(new() { TaskId = "non-existent-id" });

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Task_NotFound_If_Not_Owner()
    {
        // 1. User A creates a project and task
        var (tokenA, userIdA) = await AuthenticateUserAsync("userA_delete@example.com");
        string taskId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var project = Models.Project.Create(userIdA, "User A Project", "Desc", null);
            var task = project.AddTask("User A Task");
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            taskId = task.Id;
        }

        // 2. User B tries to delete User A's task
        var (tokenB, _) = await AuthenticateUserAsync("userB_delete@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        var response = await app.Client.DELETEAsync<DeleteTaskEndpoint, DeleteTaskRequest>(new() { TaskId = taskId });

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
