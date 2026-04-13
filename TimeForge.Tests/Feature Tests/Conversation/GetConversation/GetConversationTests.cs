using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Conversations.CreateTeamChat;
using TimeForge.Api.Features.Conversations.GetConversation;
using TimeForge.Api.Features.Teams.Create;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Conversation.GetConversation;

public class GetConversationTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task GetConversation_Success_Returns_200Ok()
    {
        // Arrange
        var email = "getter_success@example.com";
        await CreateUser(email);
        var (token, _) = await AuthenticateUserAsync(email);
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var team = await CreateTeam("Team to Get");
        var (_, createResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // Act
        var (response, result) = await app.Client.GETAsync<GetConversationEndpoint, GetConversationRequest, GetConversationResponse>(new()
        {
            Id = createResult.ConversationId
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Id.ShouldBe(createResult.ConversationId);
        result.Title.ShouldBe("Team to Get");
        result.IsTeamChat.ShouldBeTrue();
        result.TeamId.ShouldBe(team.Id);
    }

    [Fact]
    public async Task GetConversation_NonExistent_Returns_404NotFound()
    {
        // Arrange
        var email = "notfoundget@example.com";
        await CreateUser(email);
        var (token, _) = await AuthenticateUserAsync(email);
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await app.Client.GETAsync<GetConversationEndpoint, GetConversationRequest>(new()
        {
            Id = Guid.NewGuid().ToString()
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConversation_InvalidRequest_Returns_400BadRequest()
    {
        // Arrange
        var email = "invalidget@example.com";
        await CreateUser(email);
        var (token, _) = await AuthenticateUserAsync(email);
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new GetConversationRequest()
        {
            Id = ""
        };
        var response = await app.Client.GETAsync<GetConversationEndpoint, GetConversationRequest>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConversation_SecurityCheck_Should_Restrict_NonParticipants()
    {
        // Arrange
        await CreateUser("owner@example.com");
        await CreateUser("intruder@example.com");

        // Owner creates team and chat
        var (token1, _) = await AuthenticateUserAsync("owner@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var team = await CreateTeam("Owner Team");
        var (_, createResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // Intruder tries to get conversation
        var (token2, _) = await AuthenticateUserAsync("intruder@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        // Act
        var convId = createResult.ConversationId;
        var (response, _) = await app.Client.GETAsync<GetConversationEndpoint, GetConversationRequest, GetConversationResponse>(new()
        {
            Id = convId
        });
        
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden); 
    }
}
