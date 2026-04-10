using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Conversations.CreateTeamChat;
using TimeForge.Api.Features.Conversations.SendMessage;
using TimeForge.Api.Features.Teams.Create;
using TimeForge.Api.Features.Teams.RemoveMember;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Database;

namespace TimeForge.Tests.Feature_Tests.Conversation.SendMessage;

public class SendMessageTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task SendMessage_To_Team_Success_Response()
    {
        //Arrange
        //1. Create a user
        var user = await CreateUser("sender@example.com");

        //2. Authenticate
        var (token, userId) = await AuthenticateUserAsync("sender@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //3. Create a Team (Creator is automatically an owner/member)
        var team = await CreateTeam("Test Team");

        //4. Create a team conversation
        var (conversationResponse, conversationResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });

        conversationResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var conversationId = conversationResult.ConversationId;

        //Act
        //5. Send a message
        var messageResponse = await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new()
        {
            ConversationId = conversationId,
            Content = "Hello Team!"
        });

        //Assert
        messageResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendMessage_Unauthenticated_User_Throws_401_Forbidden()
    {
        //Arrange
        //1. Create unauthenticated user
        var user = await CreateUser("testUser@test.com");
        
        //2. Authenticate
        var (token, userId) = await AuthenticateUserAsync("testUser@test.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        //2. Create Team
        var team = await CreateTeam("Test Team");
        
        
        //3. Create Team Conversation
        var (conversationResponse, conversationResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });
        
        //4. Unauthenticate
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", String.Empty);
        
        conversationResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var conversationId = conversationResult.ConversationId;
        //Act
        
        //5. Send a message
        var messageResponse = await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new()
        {
            ConversationId = conversationId,
            Content = "Hello Team!"
        });
        //Assert
        messageResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendMessage_Not_Part_Of_Conversation_Throws_403_Forbidden()
    {
        //Arrange
        //1. Create 2 users
        var user = await CreateUser("testUser@test.com");
        var user2 = await CreateUser("testUser2@test.com");
        
        //2. Authenticate
        var (token, userId) = await AuthenticateUserAsync("testUser@test.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        //2. Create Team
        var team = await CreateTeam("Test Team");
        
        
        //3. Create Team Conversation
        var (conversationResponse, conversationResult) = await app.Client.POSTAsync<CreateTeamChatEndpoint, CreateTeamChatRequest, CreateTeamChatResponse>(new()
        {
            TeamId = team.Id
        });
        
        conversationResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var conversationId = conversationResult.ConversationId;
        
        //4. authenticate secondary user
        (token, userId) = await AuthenticateUserAsync("testUser2@test.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        
        //Act
        //5. Send a message
        var messageResponse = await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new()
        {
            ConversationId = conversationId,
            Content = "Hello Team!"
        });
        
        //Assert
        messageResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SendMessage_Non_Existing_Conversation_Throws_404_NotFound()
    {
        //Arrange
        //1. Create unauthenticated user
        var user = await CreateUser("testUser@test.com");
        
        //2. Authenticate
        var (token, userId) = await AuthenticateUserAsync("testUser@test.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        //Act
        
        //3. Send Message
        var messageResponse = await app.Client.POSTAsync<SendMessageEndpoint, SendMessageRequest>(new()
        {
            ConversationId = "testConversationId",
            Content = "Hello Team!"
        });
        
        //Assert
        messageResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
