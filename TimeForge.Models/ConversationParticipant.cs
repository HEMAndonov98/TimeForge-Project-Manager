using TimeForge.Models.Common;

namespace TimeForge.Models;

public class ConversationParticipant : BaseDeletableModel<string>
{
    public ConversationParticipant() : base()
    {
    }

    public string ConversationId { get; private set; } = string.Empty;
    public Conversation Conversation { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;
    public User User { get; private set; } = null!;

    public static ConversationParticipant Create(string conversationId, string userId)
    {
        return new ConversationParticipant
        {
            ConversationId = conversationId,
            UserId = userId
        };
    }
}
