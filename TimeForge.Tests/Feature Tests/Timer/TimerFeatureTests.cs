using System.Net;
using System.Net.Http.Headers;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Timer.GetStatus;
using TimeForge.Api.Features.Timer.Heartbeat;
using TimeForge.Api.Features.Timer.Start;
using TimeForge.Api.Features.Timer.Stop;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Common.Enums;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Tests.Feature_Tests.Timer;

public class TimerFeatureTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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

    private async Task<string> SeedTaskAsync(string userId)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
        var project = Models.Project.Create(userId, "Timer Test Project", "Desc", null);
        var task = project.AddTask("Timer Test Task");
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return task.Id;
    }

    [Fact]
    public async Task Timer_Full_Workflow_Test()
    {
        // 1. Setup
        var (token, userId) = await AuthenticateUserAsync("timer_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var taskId = await SeedTaskAsync(userId);

        // 2. Start Timer
        var startReq = new StartTimerRequest { TaskId = taskId };
        var (startRsp, startRes) = await app.Client.POSTAsync<StartTimerEndpoint, StartTimerRequest, StartTimerResponse>(startReq);
        startRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        startRes.State.ShouldBe("Running");
        var timerId = startRes.Id;

        // 3. Heartbeat
        var heartbeatRsp = await app.Client.POSTAsync<HeartbeatEndpoint, EndpointWithoutRequest>();
        heartbeatRsp.Response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 4. Get Status
        var (statusRsp, statusRes) = await app.Client.GETAsync<GetStatusEndpoint, TimerStatusResponse>();
        statusRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        statusRes.Id.ShouldBe(timerId);
        statusRes.State.ShouldBe("Running");

        // 5. Pause Timer
        var pauseReq = new StopTimerRequest { Finish = false };
        var (pauseRsp, pauseRes) = await app.Client.POSTAsync<StopTimerEndpoint, StopTimerRequest, StopTimerResponse>(pauseReq);
        pauseRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        pauseRes.State.ShouldBe("Paused");

        // 6. Resume (via Start)
        var (resumeRsp, resumeRes) = await app.Client.POSTAsync<StartTimerEndpoint, StartTimerRequest, StartTimerResponse>(startReq);
        resumeRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        resumeRes.State.ShouldBe("Running");
        resumeRes.Id.ShouldBe(timerId);

        // 7. Finish Timer
        var finishReq = new StopTimerRequest { Finish = true };
        var (finishRsp, finishRes) = await app.Client.POSTAsync<StopTimerEndpoint, StopTimerRequest, StopTimerResponse>(finishReq);
        finishRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        finishRes.State.ShouldBe("Completed");
    }

    [Fact]
    public async Task Timer_Recovery_Logic_Test()
    {
        // 1. Setup
        var (token, userId) = await AuthenticateUserAsync("recovery_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var taskId = await SeedTaskAsync(userId);

        // 2. Start Timer
        await app.Client.POSTAsync<StartTimerEndpoint, StartTimerRequest, StartTimerResponse>(new() { TaskId = taskId });

        // 3. Simulate Stale Heartbeat (Manually update DB)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var timer = await db.Set<TimerSession>().FirstAsync(ts => ts.UserId == userId && ts.State == TimeEntryState.Running);
            
            // We need to use reflection because properties are private set
            var lastHeartbeatProp = typeof(TimerSession).GetProperty("LastHeartbeat");
            lastHeartbeatProp!.SetValue(timer, DateTime.UtcNow.AddMinutes(-2));
            
            await db.SaveChangesAsync();
        }

        // 4. Get Status (Should trigger recovery/auto-pause)
        var (statusRsp, statusRes) = await app.Client.GETAsync<GetStatusEndpoint, TimerStatusResponse>();
        
        // 5. Assert
        statusRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        statusRes.State.ShouldBe("Paused"); // Recovery should have paused it
        statusRes.TotalSeconds.ShouldBeGreaterThanOrEqualTo(0);
    }
}
