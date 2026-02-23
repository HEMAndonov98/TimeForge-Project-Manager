using System.ComponentModel.DataAnnotations;

namespace TimeForge.Api.ToMigrate.Features.Auth.Login;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
