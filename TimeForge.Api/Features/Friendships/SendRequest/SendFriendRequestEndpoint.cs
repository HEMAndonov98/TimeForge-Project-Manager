using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Friendships.SendRequest;

public class SendFriendRequestEndpoint(TimeForgeDbContext db) : Endpoint<SendFriendRequestRequest>
{
    public override void Configure()
    {
        Post("friendships/request");
    }

    public override async Task HandleAsync(SendFriendRequestRequest req, CancellationToken ct)
    {
        var senderId = User.GetUserId();
        
        if (string.IsNullOrEmpty(senderId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (senderId == req.RecipientId)
        {
            ThrowError("You cannot send a friend request to yourself.", 400);
        }

        var recipientExists = await db.Users.AnyAsync(u => u.Id == req.RecipientId, ct);
        if (!recipientExists)
        {
            ThrowError("Recipient not found.", 404);
        }

        var existingFriendship = await db.Friendships
            .AnyAsync(f => (f.User1Id == senderId && f.User2Id == req.RecipientId) || 
                           (f.User1Id == req.RecipientId && f.User2Id == senderId), ct);

        if (existingFriendship)
        {
            ThrowError("A friendship or request already exists between these users.", 400);
        }

        var friendship = Friendship.CreateRequest(senderId, req.RecipientId);
        await db.Friendships.AddAsync(friendship, ct);
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(ct);
    }
}
