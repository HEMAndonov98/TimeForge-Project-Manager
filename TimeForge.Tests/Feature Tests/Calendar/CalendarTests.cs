using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.Features.Calendar.Create;
using TimeForge.Api.Features.Calendar.Delete;
using TimeForge.Api.Features.Calendar.Get;
using TimeForge.Api.Features.Calendar.GetAll;
using TimeForge.Api.Features.Calendar.Update;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Calendar;

public class CalendarTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
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
    public async Task Calendar_Event_Lifecycle_Test()
    {
        // 1. Authenticate
        var (token, _) = await AuthenticateUserAsync("calendar_test@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Create Event
        var createRequest = new CalendarEventRequest
        {
            Title = "Lifecycle Event",
            EventDate = DateTime.UtcNow.AddDays(1)
        };

        var (createResponse, createResult) = await app.Client.POSTAsync<CalendarEventCreateEndpoint, CalendarEventRequest, CalendarEventResponse>(createRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createResult.Title.ShouldBe(createRequest.Title);
        var eventId = createResult.Id;

        // 3. Get All Events
        var (getAllResponse, getAllResult) = await app.Client.GETAsync<GetCalendarEventsEndpoint, GetCalendarEventsRequest, GetCalendarEventsResponse>(new());
        getAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        getAllResult.Events.ShouldContain(e => e.Id == eventId);

        // 4. Get By ID
        var (getByIdResponse, getByIdResult) = await app.Client.GETAsync<GetCalendarEventEndpoint, GetEventRequest, GetEventResponse>(new GetEventRequest { Id = eventId });
        getByIdResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        getByIdResult.Id.ShouldBe(eventId);
        getByIdResult.Title.ShouldBe(createRequest.Title);

        // 5. Update Event
        var updateRequest = new CalendarEventUpdateRequest
        {
            Id = eventId,
            Title = "Updated Event Title",
            EventDate = DateTime.UtcNow.AddDays(2)
        };
        var (updateResponse, updateResult) = await app.Client.PUTAsync<CalendarEventUpdateEndpoint, CalendarEventUpdateRequest, CalendarEventUpdateResponse>(updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        updateResult.Title.ShouldBe(updateRequest.Title);

        // 6. Delete Event
        var deleteResponse = await app.Client.DELETEAsync<CalendarEventDeleteEndpoint, CalendarEventDeleteRequest>(new CalendarEventDeleteRequest { Id = eventId });
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // 7. Verify DELETED (404)
        var (getDeletedResponse, _) = await app.Client.GETAsync<GetCalendarEventEndpoint, GetEventRequest, GetEventResponse>(new GetEventRequest { Id = eventId });
        getDeletedResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Event_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var (token, _) = await AuthenticateUserAsync("invalid_calendar@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CalendarEventRequest { Title = "" };

        // Act
        var (response, _) = await app.Client.POSTAsync<CalendarEventCreateEndpoint, CalendarEventRequest, CalendarEventResponse>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Access_Other_User_Event_ReturnsForbidden()
    {
        // 1. User A creates an event
        var (tokenA, _) = await AuthenticateUserAsync("userA_cal@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenA);
        
        var (_, eventA) = await app.Client.POSTAsync<CalendarEventCreateEndpoint, CalendarEventRequest, CalendarEventResponse>(new() { Title = "User A Event", EventDate = DateTime.UtcNow });

        // 2. User B tries to access User A's event
        var (tokenB, _) = await AuthenticateUserAsync("userB_cal@example.com");
        app.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenB);

        var (response, _) = await app.Client.GETAsync<GetCalendarEventEndpoint, GetEventRequest, GetEventResponse>(new GetEventRequest { Id = eventA.Id });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
