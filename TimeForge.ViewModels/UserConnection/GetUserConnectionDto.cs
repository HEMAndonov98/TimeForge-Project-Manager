using TimeForge.Common.Enums;

namespace TimeForge.ViewModels.UserConnection;

public class GetUserConnectionDto
{
    public string FromUserID { get; set; }

    public string FromUsername { get; set; }

    public string ToUserID { get; set; }

    public string ToUsername { get; set; }

    public FriendshipStatus Status { get; set; }
}