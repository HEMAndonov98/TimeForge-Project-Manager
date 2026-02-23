 using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;
using TimeForge.Api.Features.Auth.Login; // Adjust based on your actual namespace
using TimeForge.Api.Features.Auth.Register;
using Microsoft.Extensions.DependencyInjection;
using TimeForge.Database;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Auth;

public class LoginEndpointTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
{
    [Fact, Priority(0)]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange: Ensure a user exists first
        var email = "login_success@example.com";
        var password = "SecurePassword123!";
        
        await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(new()
        {
            FirstName = "Login",
            LastName = "User",
            Email = email,
            Password = password
        });

        var request = new LoginRequest // Match your DTO name
        {
            Email = email,
            Password = password
        };

        // Act
        var (response, result) = await app.Client.POSTAsync<LoginEndpoint, LoginRequest, LoginResponse>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Token.ShouldNotBeNullOrEmpty();
        result.Email.ShouldBe(email);
    }

    [Fact, Priority(1)]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "login_success@example.com", // Created in previous test
            Password = "WrongPassword!"
        };

        // Act
        var (response, _) = await app.Client.POSTAsync<LoginEndpoint, LoginRequest, LoginResponse>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact, Priority(2)]
    public async Task Login_SoftDeletedUser_ReturnsUnauthorized()
    {
        // Arrange: Create a user and then manually "Soft Delete" them in the DB
        var email = "deleted_user@example.com";
        var password = "Password123!";
        
        var (_, user) = await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(new()
        {
            FirstName = "Deleted",
            LastName = "User",
            Email = email,
            Password = password
        });

        // Use the fixture's service provider to reach into the DB
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TimeForgeDbContext>();
            var dbUser = await db.Users.FindAsync(user.Id);
            if (dbUser != null)
            {
                db.Remove(dbUser);
                await db.SaveChangesAsync();
            }
        }

        var request = new LoginRequest { Email = email, Password = password };

        // Act
        var (response, _) = await app.Client.POSTAsync<LoginEndpoint, LoginRequest, LoginResponse>(request);

        // Assert
        // Per requirements: Deleted users cannot log in even with correct passwords
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}