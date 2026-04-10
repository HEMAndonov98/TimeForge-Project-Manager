using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Api.Hubs;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Conversations.SendMessage;

public class SendMessageValidator : Validator<SendMessageRequest>
{
    public SendMessageValidator()
    {
        RuleFor(req => req.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required.");
        
        RuleFor(req => req.Content)
            .NotEmpty()
            .WithMessage("Content is required.");
    }
}

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
            ThrowError("Conversation not found.", StatusCodes.Status404NotFound);
        }

        // Authorization check
        if (conversation.Participants.Any(p => p.UserId != senderId))
        {
            ThrowError("You are not part of this conversation.", StatusCodes.Status403Forbidden);
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
