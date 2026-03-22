namespace TimeForge.Api.Features.Conversations.GetHistory;

public class GetMessagesRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
