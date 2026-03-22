using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Conversations.GetHistory;

public class MessageDto
{
    public string Id { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class GetMessagesEndpoint(TimeForgeDbContext db) : Endpoint<GetMessagesRequest, List<MessageDto>>
{
    public override void Configure()
    {
        Get("conversations/{ConversationId}/messages");
    }

    public override async Task HandleAsync(GetMessagesRequest req, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (string.IsNullOrEmpty(currentUserId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var conversation = await db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation == null)
            ThrowError("Conversation not found.", 404);

        // Security check
        if (!conversation.Participants.Any(p => p.Id == currentUserId) && 
            !(conversation.IsTeamChat && await db.TeamMembers.AnyAsync(m => m.TeamId == conversation.TeamId && m.UserId == currentUserId, ct)))
        {
            ThrowError("Access Denied", 403);
        }

        var messages = await db.ChatMessages
            .Where(m => m.ConversationId == req.ConversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.UserName ?? "Unknown",
                Content = m.Content,
                SentAt = m.CreatedAt,
                IsRead = m.IsRead
            })
            .ToListAsync(ct);

        await Send.OkAsync(messages, ct);
    }
}
