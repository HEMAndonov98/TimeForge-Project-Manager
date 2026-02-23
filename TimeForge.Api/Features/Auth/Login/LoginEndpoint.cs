using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using TimeForge.Api.ToMigrate.Features.Auth.Login;
using TimeForge.Models;

namespace TimeForge.Api.Features.Auth.Login;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IConfiguration configuration;
    private readonly ILogger<LoginEndpoint> logger;

    public LoginEndpoint(
        UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, ILogger<LoginEndpoint> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
        this.logger = logger;
    }

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();

        Description(d => d
            .WithTags("Auth")
            .WithSummary("Authenticate user and receive JWT token")
            .Produces<LoginResponse>(200)
            .ProducesProblemDetails(401)); // Unauthorized
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // Find user by email
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user == null || user.IsDeleted)
        {
            logger.LogWarning("Login attempt with non-existent email: {Email}", req.Email);
            HttpContext.Response.StatusCode = 401;
            ThrowError("Invalid email or password");
            return;
        }

        // Check password
        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed login attempt for email: {Email}", req.Email);
            HttpContext.Response.StatusCode = 401;
            ThrowError("Invalid email or password");
            return;
        }

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Generate JWT token using FastEndpoints
        var jwtToken = JwtBearer.CreateToken(
            o =>
            {
                var key = configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(key))
                    throw new InvalidOperationException("Jwt:Key is missing from configuration");
                // Token expiration
                var expirationHours = configuration.GetValue<int>("Jwt:ExpirationHours", 24);
                o.ExpireAt = DateTime.UtcNow.AddHours(expirationHours);

                // User claims (CRITICAL: Use correct claim types for Identity compatibility)
                o.User.Claims.Add((ClaimTypes.NameIdentifier, user.Id));
                o.User.Claims.Add((ClaimTypes.Email, user.Email!));
                o.User.Claims.Add((ClaimTypes.Name, user.FullName));
                o.User.Claims.Add(("FirstName", user.FirstName));
                o.User.Claims.Add(("LastName", user.LastName));

                // Add roles if any
                foreach (var role in roles)
                {
                    o.User.Roles.Add(role);
                }

                // Issuer and Audience (must match configuration)
                o.Issuer = configuration["Jwt:Issuer"];
                o.Audience = configuration["Jwt:Audience"];
                o.SigningKey = key;
            });

        logger.LogInformation("User {Email} logged in successfully", user.Email);
        
        await Send.OkAsync(new LoginResponse
        {
            Token = jwtToken,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        }, cancellation: ct);
    }
}
