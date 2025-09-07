namespace TimeForge.ViewModels.UserConnection;

public class UserConnectionViewModel
{
    public List<GetUserConnectionDto> AcceptedRequests { get; set; } = new List<GetUserConnectionDto>();

    public List<GetUserConnectionDto> PendingReceivedRequests { get; set; } = new List<GetUserConnectionDto>();

    public List<GetUserConnectionDto> PendingSentRequests { get; set; } = new List<GetUserConnectionDto>();
}