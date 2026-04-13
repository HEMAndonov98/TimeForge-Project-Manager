using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Conversations.CreateTeamChat;
using TimeForge.Api.Features.Conversations.GetConversation;
using TimeForge.Api.Features.Conversations.GetList;
using TimeForge.Api.Features.Conversations.SendMessage;
using TimeForge.Api.Features.Teams.Create;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Conversation.GetList;

public class GetListTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task GetList_Success_Returns_Conversations()
    {
        // Arrange
        await CreateUser("list@example.com");
        var (token, _) = await AuthenticateUserAsync("list@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var team1 = await CreateTeam("Team 1");
        var (_, createResult1) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team1.Id
        });

        var team2 = await CreateTeam("Team 2");
        var (_, createResult2) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team2.Id
        });

        // Send a message to team 2 to make it more recent
        await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new()
        {
            ConversationId = createResult2.ConversationId,
            Content = "Latest message in T2"
        });

        // Act
        var (response, result) = await app.Client.GETAsync<GetConversationsEndpoint, List<ConversationListItemDto>>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Count.ShouldBeGreaterThanOrEqualTo(2);
        
        // Check ordering (latest message first)
        var t2 = result.FirstOrDefault(c => c.Id == createResult2.ConversationId);
        var t1 = result.FirstOrDefault(c => c.Id == createResult1.ConversationId);
        
        t2.ShouldNotBeNull();
        t1.ShouldNotBeNull();
        t2.LastMessageContent.ShouldBe("Latest message in T2");
        t1.LastMessageContent.ShouldBeNull(); // No messages yet
        
        var indexT2 = result.IndexOf(t2);
        var indexT1 = result.IndexOf(t1);
        indexT2.ShouldBeLessThan(indexT1);
    }

    [Fact]
    public async Task GetList_Empty_Returns_200Ok_Empty_List()
    {
        // Arrange
        await CreateUser("emptylist@example.com");
        var (token, _) = await AuthenticateUserAsync("emptylist@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var (response, result) = await app.Client.GETAsync<GetConversationsEndpoint, List<ConversationListItemDto>>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetList_Unauthenticated_Returns_401Unauthorized()
    {
        // 1. Authorize a valid user to create a conversation
        await CreateUser("setup_deauth@example.com");
        var (token, _) = await AuthenticateUserAsync("setup_deauth@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var team = await CreateTeam("Deauth Team");
        var (responseCreate, resultCreate) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // 2. Deauthorize creator of conversation
        app.Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var request = new GetConversationRequest()
        {
            Id = resultCreate.ConversationId
        };
        var response = await app.Client.GETAsync<GetConversationsEndpoint, GetConversationRequest>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
