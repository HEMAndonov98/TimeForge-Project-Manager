using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Teams.Create;
using TimeForge.Api.Features.Teams.GetAll;
using TimeForge.Api.Features.Teams.GetById;
using TimeForge.Api.Features.Teams.AddMember;
using TimeForge.Api.Features.Teams.UpdateRole;
using TimeForge.Api.Features.Teams.RemoveMember;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Common.Enums;
using TimeForge.Database;

namespace TimeForge.Tests.Feature_Tests.Teams;

public class TeamManagementTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task Complete_Team_Lifecycle_Test()
    {
        // 1. Setup Users
        var (token1, userId1) = await AuthenticateUserAsync("owner@team.com");
        var (token2, userId2) = await AuthenticateUserAsync("member@team.com");
        var (token3, userId3) = await AuthenticateUserAsync("extra@team.com");

        // 2. User 1 Creates Team
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var (createRes, createData) = await app.Client.POSTAsync<CreateTeamEndpoint, CreateTeamRequest, CreateTeamResponse>(new()
        {
            Name = "Avengers",
            Description = "Earth's Mightiest Heroes"
        });
        createRes.StatusCode.ShouldBe(HttpStatusCode.Created);
        var teamId = createData.Id;

        // 3. User 1 Adds User 2
        var addRes = await app.Client.POSTAsync<AddTeamMemberEndpoint, AddTeamMemberRequest>(new()
        {
            TeamId = teamId,
            UserEmail = "member@team.com"
        });
        addRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 4. Verify User 2 can see team details
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);
        var (getRes, getData) = await app.Client.GETAsync<GetTeamByIdEndpoint, GetTeamByIdRequest, GetTeamByIdResponse>(new() { TeamId = teamId });
        getRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        getData.Members.Count.ShouldBe(2);

        // 5. User 2 (Member) tries to add User 3 - Should fail
        var addResFail = await app.Client.POSTAsync<AddTeamMemberEndpoint, AddTeamMemberRequest>(new()
        {
            TeamId = teamId,
            UserEmail = "extra@team.com"
        });
        addResFail.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // 6. User 1 promotes User 2 to Manager
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var promoteRes = await app.Client.PATCHAsync<UpdateMemberRoleEndpoint, UpdateMemberRoleRequest>(new()
        {
            TeamId = teamId,
            UserId = userId2,
            NewRole = TeamRole.Manager
        });
        promoteRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 7. User 2 (now Manager) adds User 3 - Should succeed
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);
        var addResSuccess = await app.Client.POSTAsync<AddTeamMemberEndpoint, AddTeamMemberRequest>(new()
        {
            TeamId = teamId,
            UserEmail = "extra@team.com"
        });
        addResSuccess.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 8. User 1 transfers ownership to User 2
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var transferRes = await app.Client.PATCHAsync<UpdateMemberRoleEndpoint, UpdateMemberRoleRequest>(new()
        {
            TeamId = teamId,
            UserId = userId2,
            NewRole = TeamRole.Owner
        });
        transferRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 9. Verify User 2 is now Owner and User 1 is Manager
        var (_, verifyData) = await app.Client.GETAsync<GetTeamByIdEndpoint, GetTeamByIdRequest, GetTeamByIdResponse>(new() { TeamId = teamId });
        verifyData.Members.First(m => m.UserId == userId2).Role.ShouldBe("Owner");
        verifyData.Members.First(m => m.UserId == userId1).Role.ShouldBe("Manager");

        // 10. User 2 (new Owner) removes User 3
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);
        var removeRes = await app.Client.DELETEAsync<RemoveTeamMemberEndpoint, RemoveTeamMemberRequest>(new()
        {
            TeamId = teamId,
            UserId = userId3
        });
        removeRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 11. User 1 (now Manager) tries to leave the team - Should succeed
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var leaveRes = await app.Client.DELETEAsync<RemoveTeamMemberEndpoint, RemoveTeamMemberRequest>(new()
        {
            TeamId = teamId,
            UserId = userId1
        });
        leaveRes.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
