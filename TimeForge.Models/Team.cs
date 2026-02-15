using TimeForge.Models.Common;

namespace TimeForge.Models;

public class Team : BaseDeletableModel<string>
{
    public Team() : base()
    {
    }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private readonly List<TeamMember> _members = new();
    public IReadOnlyList<TeamMember> Members => _members.AsReadOnly();

    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();


    public static Team Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be empty");

        return new Team
        {
            Name = name,
            Description = description
        };
    }

    public TeamMember AddMember(string userId, string role = "Member")
    {
        if (_members.Any(m => m.UserId == userId))
            throw new ArgumentException("User is already a member of this team");

        var member = TeamMember.Create(this.Id, userId, role);

        _members.Add(member);
        this.MarkModified();
        return member;
    }

    public void RemoveMember(string userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            member.MarkDeleted();
            _members.Remove(member);
        }
    }

    public void UpdateMemberRole(string userId, string newRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new ArgumentException("Member not found");

        member.UpdateRole(newRole);
        this.MarkModified();
    }
}