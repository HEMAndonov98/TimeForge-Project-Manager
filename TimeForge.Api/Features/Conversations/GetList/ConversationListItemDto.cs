namespace TimeForge.Api.Features.Conversations.GetList;

public class ConversationListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsTeamChat { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageSentAt { get; set; }
    public int UnreadCount { get; set; }
}
