namespace TimeForge.Api.Features.Conversations.SendMessage;

public class SendMessageRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
