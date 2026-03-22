using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Api.Hubs;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Conversations.SendMessage;

public class SendMessageEndpoint(TimeForgeDbContext db, IHubContext<ChatHub> hubContext) : Endpoint<SendMessageRequest>
{
    public override void Configure()
    {
        Post("conversations/{ConversationId}/messages");
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
    {
        var senderId = User.GetUserId();

        if (string.IsNullOrEmpty(senderId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var conversation = await db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation == null)
        {
            ThrowError("Conversation not found.", 404);
        }

        // Authorization check
        if (!conversation.IsTeamChat && !conversation.Participants.Any(p => p.Id == senderId))
        {
            ThrowError("You are not part of this conversation.", 403);
        }
        
        // If it's a team chat, we should check if user is a member of the team
        if (conversation.IsTeamChat)
        {
            var isMember = await db.TeamMembers.AnyAsync(m => m.TeamId == conversation.TeamId && m.UserId == senderId, ct);
            if (!isMember)
            {
                ThrowError("You are not a member of this team.", 403);
            }
        }

        var message = ChatMessage.Create(senderId, conversation.Id, req.Content);
        await db.ChatMessages.AddAsync(message, ct);
        await db.SaveChangesAsync(ct);

        // Broadcast to SignalR group
        await hubContext.Clients.Group(conversation.Id).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.SenderId,
            message.Content,
            message.CreatedAt
        }, ct);

        await Send.OkAsync(ct);
    }
}
