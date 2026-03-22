using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Friendships.AcceptRequest;

public class AcceptFriendRequestEndpoint(TimeForgeDbContext db) : Endpoint<AcceptFriendRequestRequest>
{
    public override void Configure()
    {
        Post("friendships/accept");
    }

    public override async Task HandleAsync(AcceptFriendRequestRequest req, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (string.IsNullOrEmpty(currentUserId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var friendship = await db.Friendships
            .Include(f => f.User1)
            .Include(f => f.User2)
            .FirstOrDefaultAsync(f => f.Id == req.FriendshipId, ct);

        if (friendship == null)
        {
            ThrowError("Friend request not found.", 404);
        }

        if (friendship.User2Id != currentUserId)
        {
            ThrowError("You can only accept requests sent to you.", 403);
        }

        friendship.Accept();

        // Check if DM conversation already exists (e.g., if deleted and re-friended)
        var existingConversation = await db.Conversations
            .AnyAsync(c => !c.IsTeamChat && 
                           c.Participants.Any(p => p.Id == friendship.User1Id) && 
                           c.Participants.Any(p => p.Id == friendship.User2Id), ct);

        if (!existingConversation)
        {
            var conversation = Conversation.CreateDM(friendship.User1, friendship.User2);
            await db.Conversations.AddAsync(conversation, ct);
        }

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ct);
    }
}
