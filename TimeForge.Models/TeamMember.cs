using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TeamMember : BaseDeletableModel<string>
{
    public TeamMember() : base()
    {
    }
    public string TeamId { get; private set; } = string.Empty;
    public Team Team { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;
    public User User { get; private set; } = null!;
    public string Role { get; private set; } = "Member";

    internal void UpdateRole(string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
        {
            throw new ArgumentException("Role cannot be null or whitespace.", nameof(newRole));
        }

        Role = newRole;
        this.MarkModified();
    }

    public static TeamMember Create(
        string teamId,
        string userId,
        string role = "Member")
    {
        return new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            Role = role
        };
    }
}