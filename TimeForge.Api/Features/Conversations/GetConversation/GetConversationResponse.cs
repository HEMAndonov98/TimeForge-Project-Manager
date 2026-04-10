namespace TimeForge.Api.Features.Conversations.GetConversation;

public class GetConversationResponse
{
    public string Id { get; set; } = String.Empty;
    public string Title { get; set; } = String.Empty;
    public bool IsTeamChat { get; set; }
    public string? TeamId { get; set; }
}