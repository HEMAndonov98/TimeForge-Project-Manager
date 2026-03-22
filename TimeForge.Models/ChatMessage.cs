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

    public string ConversationId { get; private set; } = String.Empty;
    public Conversation Conversation { get; private set; } = null!;

    public static ChatMessage Create(string senderId, string conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));

        return new ChatMessage
        {
            SenderId = senderId,
            ConversationId = conversationId,
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