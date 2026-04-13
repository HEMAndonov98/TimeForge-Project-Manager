using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Conversations.CreateTeamChat;
using TimeForge.Api.Features.Conversations.GetHistory;
using TimeForge.Api.Features.Conversations.SendMessage;
using TimeForge.Api.Features.Teams.Create;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Conversation.GetHistory;

public class GetHistoryTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task GetHistory_Success_Returns_200Ok_With_Messages()
    {
        // Arrange
        await CreateUser("history@example.com");
        var (token, userId) = await AuthenticateUserAsync("history@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var team = await CreateTeam("History Team");
        var (_, createResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });
        var conversationId = createResult.ConversationId;

        // Send a few messages
        await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new() { ConversationId = conversationId, Content = "Msg 1" });
        await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new() { ConversationId = conversationId, Content = "Msg 2" });

        // Act
        var (response, result) = await app.Client.GETAsync<GetMessagesEndpoint, GetMessagesRequest, List<MessageDto>>(new()
        {
            ConversationId = conversationId,
            Page = 1,
            PageSize = 10
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Count.ShouldBe(2);
        result[0].Content.ShouldBe("Msg 2"); // Descending order
        result[1].Content.ShouldBe("Msg 1");
    }

    [Fact]
    public async Task GetHistory_Empty_Returns_200Ok_Empty_List()
    {
        // Arrange
        await CreateUser("emptyh@example.com");
        var (token, _) = await AuthenticateUserAsync("emptyh@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var team = await CreateTeam("Empty History Team");
        var (_, createResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        // Act
        var (response, result) = await app.Client.GETAsync<GetMessagesEndpoint, GetMessagesRequest, List<MessageDto>>(new()
        {
            ConversationId = createResult.ConversationId,
            Page = 1,
            PageSize = 10
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetHistory_AccessDenied_Returns_403Forbidden()
    {
        // Arrange
        await CreateUser("owner_h@example.com");
        await CreateUser("intruder_h@example.com");

        var (token1, _) = await AuthenticateUserAsync("owner_h@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var team = await CreateTeam("Owner H Team");
        var (_, createResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        var (token2, _) = await AuthenticateUserAsync("intruder_h@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await app.Client.GETAsync<GetMessagesEndpoint, GetMessagesRequest>(new()
        {
            ConversationId = createResult.ConversationId,
            Page = 1,
            PageSize = 10
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetHistory_NonExistent_Returns_404NotFound()
    {
        // Arrange
        await CreateUser("notfoundh@example.com");
        var (token, _) = await AuthenticateUserAsync("notfoundh@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await app.Client.GETAsync<GetMessagesEndpoint, GetMessagesRequest>(new()
        {
            ConversationId = "non-existent-id",
            Page = 1,
            PageSize = 10
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistory_Unauthenticated_Returns_401Unauthorized()
    {
        // Arrange
        app.Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await app.Client.GETAsync<GetMessagesEndpoint, GetMessagesRequest>(new()
        {
            ConversationId = "some-id",
            Page = 1,
            PageSize = 10
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
