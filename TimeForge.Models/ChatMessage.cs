using TimeForge.Models.Common;

namespace TimeForge.Models;

public class ChatMessage : BaseDeletableModel<string>
{
    public ChatMessage() : base()
    {
    }
    public string SenderId { get; private set; } = String.Empty;
    public User Sender { get; private set; } = null!;

    public string Content { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }

    public string? RecipientId { get; private set; } = String.Empty;
    public User? Recipient { get; private set; }

    public string? TeamId { get; private set; } = String.Empty;
    public Team? Team { get; private set; }

    public static ChatMessage CreateDirectMessage(string senderId, string recipientId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));

        return new ChatMessage
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content,
            IsRead = false
        };
    }

    public static ChatMessage CreateTeamMessage(string senderId, string teamId, string content)
    {

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));

        return new ChatMessage
        {
            SenderId = senderId,
            TeamId = teamId,
            Content = content,
            IsRead = false
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        this.MarkModified();
    }
}