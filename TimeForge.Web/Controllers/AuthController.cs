using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TimeForge.Models;
using TimeForge.ViewModels.User;

namespace TimeForge.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IConfiguration configuration;
    private readonly ILogger<AuthController> logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
        this.logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, request.Password))
        {
            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateToken(authClaims);

            return Ok(new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UserId = user.Id
            });
        }
        return Unauthorized();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userExists = await userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
            return BadRequest(new { message = "User already exists!" });

        User user = new()
        {
            Email = request.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = request.UserName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "User creation failed! Please check details and try again.", errors = result.Errors });
        }

        return Ok(new { message = "User created successfully!" });
    }

    private JwtSecurityToken GenerateToken(IEnumerable<Claim> authClaims)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"]!)),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}
