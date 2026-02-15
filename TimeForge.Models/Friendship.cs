using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Enums;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class Friendship : BaseDeletableModel<string>
{
    public Friendship() : base()
    {
    }
    public string User1Id { get; private set; } = String.Empty;
    public User User1 { get; private set; } = null!;

    public string User2Id { get; private set; } = String.Empty;
    public User User2 { get; private set; } = null!;

    public FriendshipStatus Status { get; private set; }

    public static Friendship CreateRequest(string requesterId, string recipientId)
    {
        return new Friendship
        {
            User1Id = requesterId,
            User2Id = recipientId,
            Status = FriendshipStatus.Pending
        };
    }

    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
            throw new ArgumentException("Can only accept pending friend requests");

        Status = FriendshipStatus.Accepted;
        this.MarkModified();
    }

    public void Reject()
    {
        Status = FriendshipStatus.Rejected;
        this.MarkModified();
    }
}