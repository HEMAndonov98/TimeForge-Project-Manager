using TimeForge.Models.Common;

namespace TimeForge.Models;

public class Conversation : BaseDeletableModel<string>
{
    public Conversation() : base()
    {
    }

    public string? Title { get; private set; }
    public bool IsTeamChat { get; private set; }
    public string? TeamId { get; private set; }
    public Team? Team { get; private set; }

    private readonly List<User> _participants = new();
    public IReadOnlyList<User> Participants => _participants.AsReadOnly();

    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    public static Conversation CreateDM(User user1, User user2)
    {
        var conversation = new Conversation
        {
            IsTeamChat = false,
            Title = $"{user1.UserName} & {user2.UserName}"
        };
        conversation._participants.Add(user1);
        conversation._participants.Add(user2);
        return conversation;
    }

    public static Conversation CreateTeamChat(Team team)
    {
        var conversation = new Conversation
        {
            IsTeamChat = true,
            TeamId = team.Id,
            Title = team.Name
        };
        // Participants for team chat are managed via Team members logic typically,
        // but for querying convenience we might sync them or just rely on TeamId.
        // Let's rely on TeamId for now to avoid duplication.
        return conversation;
    }
}
