using TimeForge.Common.Enums;
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
    public TeamRole Role { get; private set; } = TeamRole.Member;

    internal void UpdateRole(TeamRole newRole)
    {
        Role = newRole;
        this.MarkModified();
    }

    public static TeamMember Create(
        string teamId,
        string userId,
        TeamRole role = TeamRole.Member)
    {
        return new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            Role = role
        };
    }
}