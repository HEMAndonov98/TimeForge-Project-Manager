using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Common.Enums;
using TimeForge.Database;

namespace TimeForge.Api.Features.Friendships.GetList;

public class FriendDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? ConversationId { get; set; }
}

public class GetFriendsEndpoint(TimeForgeDbContext db) : EndpointWithoutRequest<List<FriendDto>>
{
    public override void Configure()
    {
        Get("friendships");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = User.GetUserId();

        if (string.IsNullOrEmpty(currentUserId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var friends = await db.Friendships
            .Where(f => (f.User1Id == currentUserId || f.User2Id == currentUserId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => new FriendDto
            {
                UserId = f.User1Id == currentUserId ? f.User2Id : f.User1Id,
                UserName = f.User1Id == currentUserId ? f.User2.UserName : f.User1.UserName,
                ConversationId = db.Conversations
                    .Where(c => !c.IsTeamChat && 
                                c.Participants.Any(p => p.Id == f.User1Id) && 
                                c.Participants.Any(p => p.Id == f.User2Id))
                    .Select(c => c.Id)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        await Send.OkAsync(friends, ct);
    }
}
