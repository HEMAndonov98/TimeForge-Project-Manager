using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;

namespace TimeForge.Api.Features.Conversations.GetList;

public class GetConversationsEndpoint(TimeForgeDbContext db) : EndpointWithoutRequest<List<ConversationListItemDto>>
{
    public override void Configure()
    {
        Get("conversations");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (string.IsNullOrEmpty(currentUserId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var conversations = await db.Conversations
            .Where(c => c.Participants.Any(p => p.Id == currentUserId))
            .Select(c => new ConversationListItemDto
            {
                Id = c.Id,
                Title = c.Title,
                IsTeamChat = c.IsTeamChat,
                LastMessageContent = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault(),
                LastMessageSentAt = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => (DateTime?)m.CreatedAt).FirstOrDefault(),
                UnreadCount = c.Messages.Count(m => m.SenderId != currentUserId && !m.IsRead)
            })
            .OrderByDescending(c => c.LastMessageSentAt ?? DateTime.MinValue)
            .ToListAsync(ct);

        await Send.OkAsync(conversations, ct);
    }
}
