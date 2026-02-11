using System.ComponentModel.DataAnnotations;

namespace TimeForge.ViewModels.User;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string Expiration { get; set; } = null!;
    public string UserId { get; set; } = null!;
}
