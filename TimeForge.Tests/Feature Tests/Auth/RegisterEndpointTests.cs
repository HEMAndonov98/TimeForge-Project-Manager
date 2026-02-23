using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using Microsoft.AspNetCore.Identity.Data;
using Shouldly;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Models;

namespace TimeForge.Tests.Feature_Tests.Auth;

// TimeForge.Tests/Features/Auth/RegisterEndpointTests.cs
public class RegisterEndpointTests(TimeForgeFixture app) : TestBase<TimeForgeFixture>
{
    [Fact, Priority(0)]
    public async Task Register_ValidData_ReturnsCreated()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var (response, result) = await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        result.Email.ShouldBe(request.Email);
        result.Id.ShouldNotBeNullOrEmpty(result.Id);
    }
    
    [Fact, Priority(1)]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var email = "duplicate@example.com";
        var request = new RegisterUserRequest { Email = email, Password = "Password123!", FirstName = "Err", LastName = "User" };

        // Act
        var (create, _) = await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(request);
        var (response, _) = await app.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(request);


        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}