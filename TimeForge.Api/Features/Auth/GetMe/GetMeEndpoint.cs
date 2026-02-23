using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using TimeForge.Models;

namespace TimeForge.Api.ToMigrate.Features.Auth.GetMe;

public class GetMeEndpoint : EndpointWithoutRequest<GetMeResponse>
{
    private readonly ILogger<GetMeEndpoint> logger;
    private readonly UserManager<User> userManager;

    public GetMeEndpoint(
        ILogger<GetMeEndpoint> logger,
        UserManager<User> userManager)
    {
        this.logger = logger;
        this.userManager = userManager;
    }
    
    public override void Configure()
    {
        Get("/auth/me");
        
        Description(
            d => d.
                WithTags("Auth")
                .WithSummary("Get current authenticated user")
                .Produces<GetMeResponse>(200)
                .ProducesProblemDetails(401)
                .ProducesProblemDetails(404)
            );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get current user id from jwt claims
        string? userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            this.logger.LogWarning("User {UserId} not found", userId);
            await Send.NotFoundAsync(ct);
            ThrowError("user id not found", 404);
        }
        
        // Retrieve user from db
        User? user = await userManager.FindByIdAsync(userId);

        if (user == null || user.IsDeleted == true)
        {
            this.logger.LogWarning("User {UserId} not found or deleted", userId);
            await Send.NotFoundAsync(ct);
            ThrowError("user id not found or deleted", 404);
        }
        
        
        // User is found and is returned
        var response =  new GetMeResponse()
        {
            Id =  user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
        };
        
        await Send.OkAsync(response, ct);
    }
}