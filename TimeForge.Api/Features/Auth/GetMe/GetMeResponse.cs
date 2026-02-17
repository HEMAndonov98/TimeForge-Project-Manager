namespace TimeForge.Api.Features.Auth.GetMe;

public class GetMeResponse
{
    public string Id { get; set; } = String.Empty;
    public string FirstName { get; set; } = String.Empty;
    public string LastName { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string? AvatarUrl { get; set; }
}