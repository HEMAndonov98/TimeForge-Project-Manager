using System.Runtime.CompilerServices;
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

    private readonly List<ConversationParticipant> _participants = new();
    public IReadOnlyList<ConversationParticipant> Participants => _participants.AsReadOnly();

    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    public static Conversation CreateDm(string title)
    {
        var conversation = new Conversation
        {
            IsTeamChat = false,
            Title = title
        };
        return conversation;
    }

    public void AddParticipant(ConversationParticipant participant)
    => this._participants.Add(participant);

    public static Conversation CreateTeamChat(Team team)
    {
        var conversation = new Conversation
        {
            IsTeamChat = true,
            TeamId = team.Id,
            Title = team.Name
        };
        return conversation;
    }
}
