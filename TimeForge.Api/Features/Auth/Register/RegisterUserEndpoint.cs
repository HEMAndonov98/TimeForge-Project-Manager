using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using TimeForge.Api.ToMigrate.Features.Auth.GetMe;
using TimeForge.Api.ToMigrate.Features.Auth.Register;
using TimeForge.Models;
using UserEntity = TimeForge.Models.User;

namespace TimeForge.Api.Features.Auth.Register;

public class RegisterUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    private readonly UserManager<User> userManager;
    private readonly ILogger<RegisterUserEndpoint> logger;

    public RegisterUserEndpoint(UserManager<User> userManager, ILogger<RegisterUserEndpoint> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();

        Description(d => d
            .WithTags("Auth")
            .WithSummary("Register a new user")
            .Produces<RegisterUserResponse>(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(409)); // Conflict - email already exist
    }

    public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
    {
        var existingUser = await userManager.FindByEmailAsync(req.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration attempt with existing email: {Email}", req.Email);
            ThrowError("Email already registered", (int)HttpStatusCode.Conflict);
            return;
        }

        // Create new user
        var user = UserEntity.CreateCustomUser(req.FirstName, req.LastName, req.Email);
        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                if (error.Code.Contains("Password"))
                {
                    logger.LogWarning("Registration failed due to password issues: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    AddError(r => r.Password, error.Description);
                }
                else if (error.Code.Contains("Email"))
                {
                    logger.LogWarning("Registration failed due to email issues: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    AddError(r => r.Email, error.Description);
                }
                else
                {
                    logger.LogWarning("Registration failed with error: {Error}", error.Description);
                    AddError(string.Empty, error.Description); // General error
                }
            }

            ThrowError("Registration failed", (int)HttpStatusCode.BadRequest);
            return;
        }

        logger.LogInformation("User {Email} registered successfully with ID {UserId}",
           user.Email, user.Id);

        await Send.CreatedAtAsync<GetMeEndpoint>(
            routeValues: null,
            responseBody: new RegisterUserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            generateAbsoluteUrl: true,
            cancellation: ct
        );
    }
}
