using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Tasks.UpdateStatus;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TaskStatus = TimeForge.Common.Enums.TaskStatus;

namespace TimeForge.Tests.Feature_Tests.Tasks;

public class UpdateTaskStatusTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task Update_Task_Status_Successfully()
    {
        // 1. Authenticate
        var (token, userId) = await AuthenticateUserAsync("task_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Project & Task (Manual Seed since no Task Create endpoint yet)
        string taskId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var project = Models.Project.Create(userId, "Integration Test Project", "Desc", null);
            var task = project.AddTask("Test Task");
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            taskId = task.Id;
        }

        // 3. Update Task Status via Patch
        var request = new UpdateTaskStatusRequest
        {
            TaskId = taskId,
            NewStatus = TaskStatus.InProgress
        };

        var (response, result) = await app.Client.PATCHAsync<UpdateTaskStatusEndpoint, UpdateTaskStatusRequest, UpdateTaskStatusResponse>(request);

        // 4. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Id.ShouldBe(taskId);
        result.Status.ShouldBe(TaskStatus.InProgress);
    }

    [Fact]
    public async Task Update_Task_Status_NotFound_If_Task_DoesNotExist()
    {
        // 1. Authenticate
        var (token, _) = await AuthenticateUserAsync("task_test_404@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Try update non-existent task
        var request = new UpdateTaskStatusRequest
        {
            TaskId = "non-existent-id",
            NewStatus = TaskStatus.Done
        };

        var (response, _) = await app.Client.PATCHAsync<UpdateTaskStatusEndpoint, UpdateTaskStatusRequest, UpdateTaskStatusResponse>(request);

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Task_Status_Forbidden_If_Not_Owner()
    {
        // 1. User A creates a project and task
        var (tokenA, userIdA) = await AuthenticateUserAsync("userA_task@example.com");
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

        // 2. User B tries to update User A's task
        var (tokenB, _) = await AuthenticateUserAsync("userB_task@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        var request = new UpdateTaskStatusRequest
        {
            TaskId = taskId,
            NewStatus = TaskStatus.Done
        };

        var (response, _) = await app.Client.PATCHAsync<UpdateTaskStatusEndpoint, UpdateTaskStatusRequest, UpdateTaskStatusResponse>(request);

        // 3. Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound); // We return 404 for forbidden projects to avoid leaks
    }
}
