using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Conversations.CreateTeamChat;
using TimeForge.Api.Features.Teams.Create;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Conversation.CreateTeamChat;

public class CreateTeamChatTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
{
    private async Task<RegisterUserResponse> CreateUser(string email)
    {
        var password = "SecurePassword123!";
        var (_, result) = await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(new()
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = password
        });

        return result;
    }

    private async Task<(string Token, string UserId)> AuthenticateUserAsync(string email)
    {
        var password = "SecurePassword123!";
        var (_, loginResult) = await app.Client.POSTAsync<LoginEndpoint, LoginRequest, LoginResponse>(new()
        {
            Email = email,
            Password = password
        });

        return (loginResult.Token, loginResult.UserId);
    }

    private async Task<CreateTeamResponse> CreateTeam(string name)
    {
        var (_, result) = await app.Client.POSTAsync<CreateTeamEndpoint, CreateTeamRequest, CreateTeamResponse>(new()
        {
            Name = name,
            Description = "Test Team Description"
        });

        return result;
    }

    [Fact]
    public async Task CreateTeamChat_Success_Returns_201Created()
    {
        // Arrange
        await CreateUser("member@example.com");
        var (token, _) = await AuthenticateUserAsync("member@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var team = await CreateTeam("New Team");

        // Act
        var (response, result) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.ConversationId.ShouldNotBeNullOrEmpty();
        result.Title.ShouldBe("New Team");
    }

    [Fact]
    public async Task CreateTeamChat_Idempotent_Returns_200Ok_For_Existing()
    {
        // Arrange
        await CreateUser("idemp@example.com");
        var (token, _) = await AuthenticateUserAsync("idemp@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var team = await CreateTeam("Existing Team");

        // First creation
        await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // Act - Second creation attempt
        var (response, result) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ConversationId.ShouldNotBeNullOrEmpty();
        result.Title.ShouldBe("Existing Team");
    }

    [Fact]
    public async Task CreateTeamChat_NonExistent_Team_Returns_404NotFound()
    {
        // Arrange
        await CreateUser("notfound@example.com");
        var (token, _) = await AuthenticateUserAsync("notfound@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest>(new()
        {
            TeamId = "non-existent-id"
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTeamChat_User_Not_Member_Returns_403Forbidden()
    {
        // Arrange
        await CreateUser("creator@example.com");
        await CreateUser("nonmember@example.com");

        // Authenticate creator to create team
        var (token1, _) = await AuthenticateUserAsync("creator@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var team = await CreateTeam("Creators Team");

        // Authenticate non-member
        var (token2, _) = await AuthenticateUserAsync("nonmember@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest>(new()
        {
            TeamId = team.Id
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTeamChat_Unauthenticated_Returns_401Unauthorized()
    {
        // Arrange
        app.Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest>(new()
        {
            TeamId = "some-id"
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTeamChat_Invalid_Request_Returns_400BadRequest()
    {
        // Arrange
        await CreateUser("valid@example.com");
        var (token, _) = await AuthenticateUserAsync("valid@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest>(new()
        {
            TeamId = "" // Empty TeamId should fail validation
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
