using System.Net;
using FastEndpoints;
using Xunit.v3;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using TimeForge.Api.Features.Auth.Register;
using TimeForge.Api.ToMigrate.Features.Auth.Register;

namespace TimeForge.Tests.Feature_Tests.Auth;

// TimeForge.Tests/Features/Auth/RegisterEndpointTests.cs
public class RegisterEndpointTests(TimeForgeFixture fixture)
{
    [Fact]
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
        var (response, result) = await fixture.Client.POSTAsync<RegisterUserEndpoint, RegisterUserRequest, RegisterUserResponse>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Email.Should().Be(request.Email);
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var email = "duplicate@example.com";
        await Fixture.CreateTestUser(email); // Setup existing user
        var request = new RegisterRequest { Email = email, Password = "Password123!", FirstName = "Err", LastName = "User" };

        // Act
        var (response, _) = await Fixture.Client.POSTAsync<RegisterEndpoint, RegisterRequest, RegisterResponse>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}